using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Stricture
{
    /// <summary>A rule scoped to the whole compilation. Override <see cref="Analyze"/>.</summary>
    public abstract class CompilationRule
    {
        /// <summary>The diagnostics this rule can report.</summary>
        public abstract IEnumerable<DiagnosticDescriptor> Descriptors { get; }

        /// <summary>Analyzes the compilation.</summary>
        public abstract void Analyze(CompilationRuleContext ctx);
    }
}
