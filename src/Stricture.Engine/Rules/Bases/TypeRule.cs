using Microsoft.CodeAnalysis;

namespace Stricture
{
    /// <summary>A rule scoped to a single named type. Override <see cref="Analyze"/>.</summary>
    public abstract class TypeRule
    {
        /// <summary>The diagnostic this rule can report.</summary>
        public abstract DiagnosticDescriptor Descriptor { get; }

        /// <summary>Analyzes one type.</summary>
        public abstract void Analyze(TypeRuleContext ctx);
    }
}
