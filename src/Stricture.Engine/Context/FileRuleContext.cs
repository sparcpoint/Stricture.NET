using System;
using Microsoft.CodeAnalysis;

namespace Stricture
{
    /// <summary>The per-file context (syntactic).</summary>
    public sealed class FileRuleContext
    {
        private readonly Action<Diagnostic> _report;

        /// <summary>Creates a context for one syntax tree.</summary>
        public FileRuleContext(SyntaxTree tree, SharedContext shared, Action<Diagnostic> report)
        {
            Tree = tree ?? throw new ArgumentNullException(nameof(tree));
            Shared = shared ?? throw new ArgumentNullException(nameof(shared));
            _report = report ?? throw new ArgumentNullException(nameof(report));
        }

        /// <summary>The syntax tree under analysis.</summary>
        public SyntaxTree Tree { get; }

        /// <summary>The shared compilation context.</summary>
        public SharedContext Shared { get; }

        /// <summary>Reports a diagnostic at the given location.</summary>
        public void Report(DiagnosticDescriptor descriptor, Location location, params object[] messageArgs) =>
            _report(Diagnostic.Create(descriptor, location, messageArgs));
    }
}
