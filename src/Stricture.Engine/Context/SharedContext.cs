using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Stricture
{
    /// <summary>
    /// The expensive, compilation-stable context built once per compilation: the project directory,
    /// the parsed policy (all <c>Stricture</c> assembly attributes), and config-validation results.
    /// </summary>
    public sealed class SharedContext
    {
        private SharedContext(Compilation compilation, string? projectDir, Policy policy, INamedTypeSymbol? publicApiAttribute)
        {
            Compilation = compilation;
            ProjectDir = projectDir;
            Policy = policy;
            PublicApiAttribute = publicApiAttribute;
        }

        /// <summary>The compilation under analysis.</summary>
        public Compilation Compilation { get; }

        /// <summary>The consumer's project directory, if exposed via <c>build_property.ProjectDir</c>.</summary>
        public string? ProjectDir { get; }

        /// <summary>The resolved <c>Stricture.PublicApiAttribute</c> symbol, if present.</summary>
        internal INamedTypeSymbol? PublicApiAttribute { get; }

        /// <summary>The parsed policy.</summary>
        internal Policy Policy { get; }

        /// <summary>Builds the shared context once for a compilation.</summary>
        public static SharedContext Build(Compilation compilation, AnalyzerOptions options)
        {
            if (compilation is null)
            {
                throw new ArgumentNullException(nameof(compilation));
            }

            string? projectDir = null;
            if (options?.AnalyzerConfigOptionsProvider?.GlobalOptions is { } global
                && global.TryGetValue("build_property.ProjectDir", out var dir)
                && !string.IsNullOrEmpty(dir))
            {
                projectDir = dir;
            }

            var policy = ParsePolicy(compilation);
            var publicApi = compilation.GetTypeByMetadataName(WellKnownNames.PublicApi);
            return new SharedContext(compilation, projectDir, policy, publicApi);
        }

        private static Policy ParsePolicy(Compilation compilation)
        {
            var structures = ImmutableArray.CreateBuilder<FolderStructurePolicy>();
            var typeFolders = ImmutableArray.CreateBuilder<TypeFolderPolicy>();
            var coLocate = ImmutableArray.CreateBuilder<CoLocateGroup>();
            var bannedTypes = ImmutableArray.CreateBuilder<BanTypeEntry>();
            var bannedNamespaces = ImmutableArray.CreateBuilder<BanNamespaceEntry>();
            var bannedPackages = ImmutableArray.CreateBuilder<BanPackageEntry>();
            var oneTypePerFile = false;
            var requireSharedStem = true;
            var defaultVisibilityInternal = false;

            foreach (var attr in compilation.Assembly.GetAttributes())
            {
                var name = attr.AttributeClass?.ToDisplayString();
                if (name is null)
                {
                    continue;
                }

                switch (name)
                {
                    case WellKnownNames.FolderStructure:
                        structures.Add(new FolderStructurePolicy(
                            GetCtorString(attr, 0) ?? string.Empty,
                            GetNamedString(attr, "Pattern") ?? string.Empty,
                            GetNamedString(attr, "Fallback")));
                        break;

                    case WellKnownNames.TypeFolder:
                        typeFolders.Add(new TypeFolderPolicy(
                            GetCtorString(attr, 0) ?? string.Empty,
                            (TypeShape)GetNamedEnum(attr, "Kind"),
                            GetNamedType(attr, "DerivesFrom"),
                            GetNamedType(attr, "Implements"),
                            GetNamedType(attr, "MarkedWith"),
                            GetNamedString(attr, "EndsWith"),
                            GetNamedBool(attr, "IsRecord"),
                            (RuleTier)GetNamedEnum(attr, "Tier")));
                        break;

                    case WellKnownNames.OneTypePerFile:
                        oneTypePerFile = true;
                        requireSharedStem = GetNamedBool(attr, "RequireSharedStem", defaultValue: true);
                        break;

                    case WellKnownNames.CoLocateBySuffix:
                        coLocate.Add(new CoLocateGroup(GetCtorStringArray(attr, 0)));
                        break;

                    case WellKnownNames.DefaultVisibility:
                        defaultVisibilityInternal = (Visibility)GetCtorEnum(attr, 0) == Visibility.Internal;
                        break;

                    case WellKnownNames.BanType:
                        bannedTypes.Add(new BanTypeEntry(
                            GetNamedType(attr, "Type"),
                            GetNamedString(attr, "FullyQualifiedName"),
                            GetNamedString(attr, "Message")));
                        break;

                    case WellKnownNames.BanNamespace:
                        bannedNamespaces.Add(new BanNamespaceEntry(
                            GetCtorString(attr, 0) ?? string.Empty,
                            GetNamedString(attr, "Message")));
                        break;

                    case WellKnownNames.BanPackage:
                        bannedPackages.Add(new BanPackageEntry(
                            GetCtorString(attr, 0) ?? string.Empty,
                            GetNamedString(attr, "Message")));
                        break;

                    default:
                        break;
                }
            }

            var configIssues = ValidateConfig(structures, typeFolders, coLocate);

            return new Policy(
                structures.ToImmutable(),
                typeFolders.ToImmutable(),
                coLocate.ToImmutable(),
                bannedTypes.ToImmutable(),
                bannedNamespaces.ToImmutable(),
                bannedPackages.ToImmutable(),
                oneTypePerFile,
                requireSharedStem,
                defaultVisibilityInternal,
                configIssues);
        }

        private static ImmutableArray<string> ValidateConfig(
            ImmutableArray<FolderStructurePolicy>.Builder structures,
            ImmutableArray<TypeFolderPolicy>.Builder typeFolders,
            ImmutableArray<CoLocateGroup>.Builder coLocate)
        {
            var issues = ImmutableArray.CreateBuilder<string>();

            // Pattern must contain {category}; duplicate roots are contradictory.
            var seenRoots = new HashSet<string>(StringComparer.Ordinal);
            foreach (var s in structures)
            {
                if (!PathUtil.ContainsOrdinal(s.Pattern, "{category}"))
                {
                    issues.Add($"FolderStructure root '{s.Root}' has pattern '{s.Pattern}' which is missing the '{{category}}' slot.");
                }

                if (!seenRoots.Add(s.Root))
                {
                    issues.Add($"Duplicate FolderStructure root '{s.Root}'.");
                }
            }

            // Suffixes in one co-location group that map (via TypeFolder.EndsWith) to different folders.
            foreach (var group in coLocate)
            {
                var folders = new HashSet<string>(StringComparer.Ordinal);
                foreach (var suffix in group.Suffixes)
                {
                    foreach (var tf in typeFolders)
                    {
                        if (!string.IsNullOrEmpty(tf.EndsWith)
                            && string.Equals(tf.EndsWith, suffix, StringComparison.Ordinal))
                        {
                            folders.Add(tf.Folder);
                        }
                    }
                }

                if (folders.Count > 1)
                {
                    issues.Add($"CoLocateBySuffix group [{string.Join(", ", group.Suffixes)}] maps to multiple folders: {string.Join(", ", folders)}.");
                }
            }

            return issues.ToImmutable();
        }

        private static string? GetCtorString(AttributeData attr, int index)
        {
            if (attr.ConstructorArguments.Length > index)
            {
                return attr.ConstructorArguments[index].Value as string;
            }

            return null;
        }

        private static int GetCtorEnum(AttributeData attr, int index)
        {
            if (attr.ConstructorArguments.Length > index && attr.ConstructorArguments[index].Value is { } v)
            {
                return Convert.ToInt32(v, System.Globalization.CultureInfo.InvariantCulture);
            }

            return 0;
        }

        private static ImmutableArray<string> GetCtorStringArray(AttributeData attr, int index)
        {
            if (attr.ConstructorArguments.Length > index)
            {
                var arg = attr.ConstructorArguments[index];
                if (arg.Kind == TypedConstantKind.Array)
                {
                    var builder = ImmutableArray.CreateBuilder<string>();
                    foreach (var element in arg.Values)
                    {
                        if (element.Value is string s)
                        {
                            builder.Add(s);
                        }
                    }

                    return builder.ToImmutable();
                }
            }

            return ImmutableArray<string>.Empty;
        }

        private static TypedConstant? GetNamed(AttributeData attr, string name)
        {
            foreach (var pair in attr.NamedArguments)
            {
                if (string.Equals(pair.Key, name, StringComparison.Ordinal))
                {
                    return pair.Value;
                }
            }

            return null;
        }

        private static string? GetNamedString(AttributeData attr, string name) =>
            GetNamed(attr, name)?.Value as string;

        private static bool GetNamedBool(AttributeData attr, string name, bool defaultValue = false)
        {
            var tc = GetNamed(attr, name);
            return tc?.Value is bool b ? b : defaultValue;
        }

        private static int GetNamedEnum(AttributeData attr, string name)
        {
            var tc = GetNamed(attr, name);
            return tc?.Value is { } v ? Convert.ToInt32(v, System.Globalization.CultureInfo.InvariantCulture) : 0;
        }

        private static INamedTypeSymbol? GetNamedType(AttributeData attr, string name) =>
            GetNamed(attr, name)?.Value as INamedTypeSymbol;
    }
}
