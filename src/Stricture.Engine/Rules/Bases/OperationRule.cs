using Microsoft.CodeAnalysis;

namespace Stricture
{
    /// <summary>A rule scoped to a single operation. Override <see cref="Analyze"/>.</summary>
    public abstract class OperationRule
    {
        /// <summary>The diagnostic this rule can report.</summary>
        public abstract DiagnosticDescriptor Descriptor { get; }

        /// <summary>Analyzes one operation.</summary>
        public abstract void Analyze(OperationRuleContext ctx);
    }
}
