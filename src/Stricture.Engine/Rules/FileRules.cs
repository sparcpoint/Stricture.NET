using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Stricture.Rules
{
    /// <summary>A top-level type declaration discovered syntactically.</summary>
    internal readonly struct TopLevelType
    {
        public TopLevelType(string name, Location location)
        {
            Name = name;
            Location = location;
        }

        public string Name { get; }

        public Location Location { get; }
    }

    /// <summary>Syntactic helpers shared by the file rules.</summary>
    internal static class FileSyntax
    {
        /// <summary>Collects the top-level type declarations in a syntax tree.</summary>
        public static List<TopLevelType> GetTopLevelTypes(SyntaxNode root)
        {
            var result = new List<TopLevelType>();
            Collect(root, result);
            return result;
        }

        private static void Collect(SyntaxNode container, List<TopLevelType> result)
        {
            IEnumerable<MemberDeclarationSyntax> members;
            switch (container)
            {
                case CompilationUnitSyntax cu:
                    members = cu.Members;
                    break;
                case BaseNamespaceDeclarationSyntax ns:
                    members = ns.Members;
                    break;
                default:
                    return;
            }

            foreach (var member in members)
            {
                switch (member)
                {
                    case BaseNamespaceDeclarationSyntax nested:
                        Collect(nested, result);
                        break;
                    case BaseTypeDeclarationSyntax type:
                        result.Add(new TopLevelType(type.Identifier.Text, type.Identifier.GetLocation()));
                        break;
                    case DelegateDeclarationSyntax del:
                        result.Add(new TopLevelType(del.Identifier.Text, del.Identifier.GetLocation()));
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>The first suffix in <paramref name="group"/> that <paramref name="name"/> ends with, or null.</summary>
        public static string? MatchingSuffix(string name, CoLocateGroup group)
        {
            foreach (var suffix in group.Suffixes)
            {
                if (!string.IsNullOrEmpty(suffix) && name.EndsWith(suffix, StringComparison.Ordinal))
                {
                    return suffix;
                }
            }

            return null;
        }

        /// <summary>The group index whose suffix <paramref name="name"/> ends with, or -1.</summary>
        public static int GroupIndexOf(string name, ImmutableArray<CoLocateGroup> groups)
        {
            for (var i = 0; i < groups.Length; i++)
            {
                if (MatchingSuffix(name, groups[i]) != null)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>True when a single group covers every name (and stems match when required).</summary>
        public static bool IsValidSingleGroup(IReadOnlyList<TopLevelType> types, ImmutableArray<CoLocateGroup> groups, bool requireSharedStem)
        {
            foreach (var group in groups)
            {
                var allMatch = true;
                string? stem = null;
                var stemsEqual = true;

                foreach (var t in types)
                {
                    var suffix = MatchingSuffix(t.Name, group);
                    if (suffix is null)
                    {
                        allMatch = false;
                        break;
                    }

                    var thisStem = t.Name.Substring(0, t.Name.Length - suffix.Length);
                    if (stem is null)
                    {
                        stem = thisStem;
                    }
                    else if (!string.Equals(stem, thisStem, StringComparison.Ordinal))
                    {
                        stemsEqual = false;
                    }
                }

                if (allMatch && (!requireSharedStem || stemsEqual))
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>ARCH2001: more than one top-level type in a file that is not a permitted co-location group.</summary>
    public sealed class OneTypePerFileRule : FileRule
    {
        /// <inheritdoc />
        public override DiagnosticDescriptor Descriptor => Descriptors.Arch2001;

        /// <inheritdoc />
        public override void Analyze(FileRuleContext ctx)
        {
            var policy = ctx.Shared.Policy;
            if (!policy.OneTypePerFile)
            {
                return;
            }

            var types = FileSyntax.GetTopLevelTypes(ctx.Tree.GetRoot());
            if (types.Count <= 1)
            {
                return;
            }

            if (FileSyntax.IsValidSingleGroup(types, policy.CoLocateGroups, policy.RequireSharedStem))
            {
                return;
            }

            // If every type is a co-location candidate, ARCH2002 owns the deeper diagnosis.
            var allKnown = true;
            foreach (var t in types)
            {
                if (FileSyntax.GroupIndexOf(t.Name, policy.CoLocateGroups) < 0)
                {
                    allKnown = false;
                    break;
                }
            }

            if (allKnown && policy.CoLocateGroups.Length > 0)
            {
                return;
            }

            // Report the odd-one-out: the last type that is not a co-location candidate.
            TopLevelType oddOne = types[types.Count - 1];
            for (var i = types.Count - 1; i >= 0; i--)
            {
                if (FileSyntax.GroupIndexOf(types[i].Name, policy.CoLocateGroups) < 0)
                {
                    oddOne = types[i];
                    break;
                }
            }

            ctx.Report(Descriptor, oddOne.Location, oddOne.Name);
        }
    }

    /// <summary>ARCH2002: co-located types mix suffix groups or (when required) have mismatched stems.</summary>
    public sealed class CoLocationRule : FileRule
    {
        /// <inheritdoc />
        public override DiagnosticDescriptor Descriptor => Descriptors.Arch2002;

        /// <inheritdoc />
        public override void Analyze(FileRuleContext ctx)
        {
            var policy = ctx.Shared.Policy;
            if (policy.CoLocateGroups.Length == 0)
            {
                return;
            }

            var types = FileSyntax.GetTopLevelTypes(ctx.Tree.GetRoot());
            if (types.Count <= 1)
            {
                return;
            }

            // Only diagnose when every type is a co-location candidate (otherwise ARCH2001 owns it).
            var distinctGroups = new HashSet<int>();
            foreach (var t in types)
            {
                var gi = FileSyntax.GroupIndexOf(t.Name, policy.CoLocateGroups);
                if (gi < 0)
                {
                    return;
                }

                distinctGroups.Add(gi);
            }

            if (FileSyntax.IsValidSingleGroup(types, policy.CoLocateGroups, policy.RequireSharedStem))
            {
                return;
            }

            string detail;
            if (distinctGroups.Count > 1)
            {
                detail = "types belong to different co-location groups.";
            }
            else
            {
                detail = "co-located types must share a common stem.";
            }

            ctx.Report(Descriptor, types[0].Location, detail);
        }
    }
}
