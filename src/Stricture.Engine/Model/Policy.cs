using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Stricture
{
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
