using Microsoft.CodeAnalysis;

namespace Stricture
{
    /// <summary>A declared <c>BanNamespace</c>.</summary>
    internal sealed class BanNamespaceEntry
    {
        public BanNamespaceEntry(string ns, string? message, DiagnosticSeverity severity)
        {
            Namespace = ns;
            Message = message;
            Severity = severity;
        }

        public string Namespace { get; }

        public string? Message { get; }

        /// <summary>The severity violations of this ban are reported at.</summary>
        public DiagnosticSeverity Severity { get; }
    }
}
