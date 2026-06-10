using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Stricture
{
    /// <summary>A declared <c>FolderStructure</c>: a root + path pattern + optional fallback category.</summary>
    internal sealed class FolderStructurePolicy
    {
        public FolderStructurePolicy(string root, string pattern, string? fallback)
        {
            Root = root ?? string.Empty;
            Pattern = pattern ?? string.Empty;
            Fallback = fallback;
            Tokens = SplitTokens(Pattern);
            CategoryIndex = IndexOfCategory(Tokens);
        }

        public string Root { get; }

        public string Pattern { get; }

        public string? Fallback { get; }

        public IReadOnlyList<string> Tokens { get; }

        public int CategoryIndex { get; }

        public bool HasCategoryToken => CategoryIndex >= 0;

        private static List<string> SplitTokens(string pattern)
        {
            var parts = pattern.Split('/');
            var result = new List<string>(parts.Length);
            foreach (var p in parts)
            {
                if (p.Length > 0)
                {
                    result.Add(p);
                }
            }

            return result;
        }

        private static int IndexOfCategory(IReadOnlyList<string> tokens)
        {
            for (var i = 0; i < tokens.Count; i++)
            {
                if (string.Equals(tokens[i], "{category}", StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }
    }

    /// <summary>A declared <c>TypeFolder</c>: a classification signal mapped to a category folder.</summary>
    internal sealed class TypeFolderPolicy
    {
        public TypeFolderPolicy(
            string folder,
            TypeShape kind,
            INamedTypeSymbol? derivesFrom,
            INamedTypeSymbol? implements,
            INamedTypeSymbol? markedWith,
            string? endsWith,
            bool isRecord,
            RuleTier tier)
        {
            Folder = folder;
            Kind = kind;
            DerivesFrom = derivesFrom;
            Implements = implements;
            MarkedWith = markedWith;
            EndsWith = endsWith;
            IsRecord = isRecord;
            Tier = tier;
        }

        public string Folder { get; }

        public TypeShape Kind { get; }

        public INamedTypeSymbol? DerivesFrom { get; }

        public INamedTypeSymbol? Implements { get; }

        public INamedTypeSymbol? MarkedWith { get; }

        public string? EndsWith { get; }

        public bool IsRecord { get; }

        public RuleTier Tier { get; }

        /// <summary>The tier, deriving from the signal when <see cref="RuleTier.Auto"/>.</summary>
        public RuleTier EffectiveTier
        {
            get
            {
                if (Tier != RuleTier.Auto)
                {
                    return Tier;
                }

                return (Kind != TypeShape.None || DerivesFrom != null)
                    ? RuleTier.Structural
                    : RuleTier.Convention;
            }
        }

        /// <summary>True when every specified signal matches <paramref name="type"/>.</summary>
        public bool Matches(INamedTypeSymbol type)
        {
            var any = false;

            if (Kind != TypeShape.None)
            {
                any = true;
                if (!MatchesKind(type, Kind))
                {
                    return false;
                }
            }

            if (DerivesFrom != null)
            {
                any = true;
                if (!DerivesFromSymbol(type, DerivesFrom))
                {
                    return false;
                }
            }

            if (Implements != null)
            {
                any = true;
                if (!ImplementsSymbol(type, Implements))
                {
                    return false;
                }
            }

            if (MarkedWith != null)
            {
                any = true;
                if (!HasAttributeSymbol(type, MarkedWith))
                {
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(EndsWith))
            {
                any = true;
                if (!type.Name.EndsWith(EndsWith!, StringComparison.Ordinal))
                {
                    return false;
                }
            }

            if (IsRecord)
            {
                any = true;
                if (!type.IsRecord)
                {
                    return false;
                }
            }

            return any;
        }

        private static bool MatchesKind(INamedTypeSymbol type, TypeShape kind)
        {
            switch (kind)
            {
                case TypeShape.Class: return type.TypeKind == TypeKind.Class && !type.IsRecord;
                case TypeShape.Struct: return type.TypeKind == TypeKind.Struct && !type.IsRecord;
                case TypeShape.Interface: return type.TypeKind == TypeKind.Interface;
                case TypeShape.Enum: return type.TypeKind == TypeKind.Enum;
                case TypeShape.Record: return type.IsRecord;
                case TypeShape.Delegate: return type.TypeKind == TypeKind.Delegate;
                default: return false;
            }
        }

        private static bool DerivesFromSymbol(INamedTypeSymbol type, INamedTypeSymbol baseType)
        {
            var current = type.BaseType;
            while (current != null)
            {
                if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, baseType.OriginalDefinition))
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        }

        private static bool ImplementsSymbol(INamedTypeSymbol type, INamedTypeSymbol iface)
        {
            foreach (var i in type.AllInterfaces)
            {
                if (SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, iface.OriginalDefinition))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasAttributeSymbol(INamedTypeSymbol type, INamedTypeSymbol attr)
        {
            foreach (var a in type.GetAttributes())
            {
                if (a.AttributeClass != null
                    && SymbolEqualityComparer.Default.Equals(a.AttributeClass.OriginalDefinition, attr.OriginalDefinition))
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>A declared <c>CoLocateBySuffix</c> group.</summary>
    internal sealed class CoLocateGroup
    {
        public CoLocateGroup(ImmutableArray<string> suffixes)
        {
            Suffixes = suffixes;
        }

        public ImmutableArray<string> Suffixes { get; }
    }

    /// <summary>A declared <c>BanType</c>.</summary>
    internal sealed class BanTypeEntry
    {
        public BanTypeEntry(INamedTypeSymbol? symbol, string? fullyQualifiedName, string? message)
        {
            Symbol = symbol;
            FullyQualifiedName = fullyQualifiedName;
            Message = message;
        }

        public INamedTypeSymbol? Symbol { get; }

        public string? FullyQualifiedName { get; }

        public string? Message { get; }
    }

    /// <summary>A declared <c>BanNamespace</c>.</summary>
    internal sealed class BanNamespaceEntry
    {
        public BanNamespaceEntry(string ns, string? message)
        {
            Namespace = ns;
            Message = message;
        }

        public string Namespace { get; }

        public string? Message { get; }
    }

    /// <summary>A declared <c>BanPackage</c>.</summary>
    internal sealed class BanPackageEntry
    {
        public BanPackageEntry(string assemblyName, string? message)
        {
            AssemblyName = assemblyName;
            Message = message;
        }

        public string AssemblyName { get; }

        public string? Message { get; }
    }

    /// <summary>The parsed, compilation-stable set of all <c>Stricture</c> assembly attributes.</summary>
    internal sealed class Policy
    {
        public Policy(
            ImmutableArray<FolderStructurePolicy> structures,
            ImmutableArray<TypeFolderPolicy> typeFolders,
            ImmutableArray<CoLocateGroup> coLocateGroups,
            ImmutableArray<BanTypeEntry> bannedTypes,
            ImmutableArray<BanNamespaceEntry> bannedNamespaces,
            ImmutableArray<BanPackageEntry> bannedPackages,
            bool oneTypePerFile,
            bool requireSharedStem,
            bool defaultVisibilityIsInternal,
            ImmutableArray<string> configIssues)
        {
            Structures = structures;
            TypeFolders = typeFolders;
            CoLocateGroups = coLocateGroups;
            BannedTypes = bannedTypes;
            BannedNamespaces = bannedNamespaces;
            BannedPackages = bannedPackages;
            OneTypePerFile = oneTypePerFile;
            RequireSharedStem = requireSharedStem;
            DefaultVisibilityIsInternal = defaultVisibilityIsInternal;
            ConfigIssues = configIssues;
        }

        public ImmutableArray<FolderStructurePolicy> Structures { get; }

        public ImmutableArray<TypeFolderPolicy> TypeFolders { get; }

        public ImmutableArray<CoLocateGroup> CoLocateGroups { get; }

        public ImmutableArray<BanTypeEntry> BannedTypes { get; }

        public ImmutableArray<BanNamespaceEntry> BannedNamespaces { get; }

        public ImmutableArray<BanPackageEntry> BannedPackages { get; }

        public bool OneTypePerFile { get; }

        public bool RequireSharedStem { get; }

        public bool DefaultVisibilityIsInternal { get; }

        public ImmutableArray<string> ConfigIssues { get; }

        /// <summary>
        /// Resolves the category folder for a type using the precedence in §6.6:
        /// structural matches first (last declared wins), then convention matches (last wins), else null.
        /// </summary>
        public string? ResolveCategory(INamedTypeSymbol type)
        {
            string? structural = null;
            string? convention = null;

            foreach (var rule in TypeFolders)
            {
                if (!rule.Matches(type))
                {
                    continue;
                }

                if (rule.EffectiveTier == RuleTier.Structural)
                {
                    structural = rule.Folder; // last declared wins
                }
                else if (rule.EffectiveTier == RuleTier.Convention)
                {
                    convention = rule.Folder;
                }
            }

            return structural ?? convention;
        }

        /// <summary>
        /// Locates the structure whose root segment appears in the file's directory path and extracts
        /// the actual category folder. Returns <see langword="null"/> if the path is under no structure.
        /// </summary>
        public StructureMatch? MatchStructure(string? filePath)
        {
            var segments = PathUtil.GetDirectorySegments(filePath);
            if (segments.Count == 0)
            {
                return null;
            }

            foreach (var structure in Structures)
            {
                var rootIndex = IndexOfSegment(segments, structure.Root);
                if (rootIndex < 0)
                {
                    continue;
                }

                var after = segments.Count - (rootIndex + 1);
                if (after == structure.Tokens.Count && structure.HasCategoryToken)
                {
                    var category = segments[rootIndex + 1 + structure.CategoryIndex];
                    return new StructureMatch(structure.Root, structure.Pattern, structure.Fallback, category, shapeMatches: true);
                }

                return new StructureMatch(structure.Root, structure.Pattern, structure.Fallback, actualCategoryFolder: null, shapeMatches: false);
            }

            return null;
        }

        private static int IndexOfSegment(IReadOnlyList<string> segments, string root)
        {
            for (var i = 0; i < segments.Count; i++)
            {
                if (string.Equals(segments[i], root, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
