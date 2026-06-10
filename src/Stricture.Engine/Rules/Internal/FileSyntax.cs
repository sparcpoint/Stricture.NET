using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Stricture.Rules
{
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
}
