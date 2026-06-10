using System.Collections.Generic;
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

    /// <summary>A rule scoped to a single syntax tree (file). Override <see cref="Analyze"/>.</summary>
    public abstract class FileRule
    {
        /// <summary>The diagnostic this rule can report.</summary>
        public abstract DiagnosticDescriptor Descriptor { get; }

        /// <summary>Analyzes one file.</summary>
        public abstract void Analyze(FileRuleContext ctx);
    }

    /// <summary>A rule scoped to a single operation. Override <see cref="Analyze"/>.</summary>
    public abstract class OperationRule
    {
        /// <summary>The diagnostic this rule can report.</summary>
        public abstract DiagnosticDescriptor Descriptor { get; }

        /// <summary>Analyzes one operation.</summary>
        public abstract void Analyze(OperationRuleContext ctx);
    }

    /// <summary>A rule scoped to the whole compilation. Override <see cref="Analyze"/>.</summary>
    public abstract class CompilationRule
    {
        /// <summary>The diagnostics this rule can report.</summary>
        public abstract IEnumerable<DiagnosticDescriptor> Descriptors { get; }

        /// <summary>Analyzes the compilation.</summary>
        public abstract void Analyze(CompilationRuleContext ctx);
    }
}
