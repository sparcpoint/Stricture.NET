using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Stricture
{
    /// <summary>A declared <c>FolderStructure</c>: a root + path pattern + optional fallback category.</summary>
    internal sealed class FolderStructurePolicy
    {
        public FolderStructurePolicy(string root, string pattern, string? fallback, DiagnosticSeverity severity)
        {
            Root = root ?? string.Empty;
            Pattern = pattern ?? string.Empty;
            Fallback = fallback;
            Severity = severity;
            Tokens = SplitTokens(Pattern);
            CategoryIndex = IndexOfCategory(Tokens);
        }

        public string Root { get; }

        public string Pattern { get; }

        public string? Fallback { get; }

        /// <summary>The severity violations under this structure are reported at.</summary>
        public DiagnosticSeverity Severity { get; }

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
}
