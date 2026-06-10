using Microsoft.CodeAnalysis;

namespace Stricture
{
    /// <summary>A rule scoped to a single syntax tree (file). Override <see cref="Analyze"/>.</summary>
    public abstract class FileRule
    {
        /// <summary>The diagnostic this rule can report.</summary>
        public abstract DiagnosticDescriptor Descriptor { get; }

        /// <summary>Analyzes one file.</summary>
        public abstract void Analyze(FileRuleContext ctx);
    }
}
