using Microsoft.CodeAnalysis;

namespace Stricture
{
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
}
