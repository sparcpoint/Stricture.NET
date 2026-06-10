using Microsoft.CodeAnalysis;

namespace Stricture
{
    /// <summary>A declared <c>BanType</c>.</summary>
    internal sealed class BanTypeEntry
    {
        public BanTypeEntry(INamedTypeSymbol? symbol, string? fullyQualifiedName, string? message, DiagnosticSeverity severity)
        {
            Symbol = symbol;
            FullyQualifiedName = fullyQualifiedName;
            Message = message;
            Severity = severity;
        }

        public INamedTypeSymbol? Symbol { get; }

        public string? FullyQualifiedName { get; }

        public string? Message { get; }

        /// <summary>The severity violations of this ban are reported at.</summary>
        public DiagnosticSeverity Severity { get; }
    }
}
