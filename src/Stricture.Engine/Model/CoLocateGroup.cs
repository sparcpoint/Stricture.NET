using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Stricture
{
    /// <summary>A declared <c>CoLocateBySuffix</c> group.</summary>
    internal sealed class CoLocateGroup
    {
        public CoLocateGroup(ImmutableArray<string> suffixes, DiagnosticSeverity severity)
        {
            Suffixes = suffixes;
            Severity = severity;
        }

        public ImmutableArray<string> Suffixes { get; }

        /// <summary>The severity violations of this co-location group are reported at.</summary>
        public DiagnosticSeverity Severity { get; }
    }
}
