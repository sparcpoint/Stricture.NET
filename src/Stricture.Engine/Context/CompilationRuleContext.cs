using System;
using Microsoft.CodeAnalysis;

namespace Stricture
{
    /// <summary>The whole-compilation context.</summary>
    public sealed class CompilationRuleContext
    {
        private readonly Action<Diagnostic> _report;

        /// <summary>Creates a context for the compilation.</summary>
        public CompilationRuleContext(Compilation compilation, SharedContext shared, Action<Diagnostic> report)
        {
            Compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));
            Shared = shared ?? throw new ArgumentNullException(nameof(shared));
            _report = report ?? throw new ArgumentNullException(nameof(report));
        }

        /// <summary>The compilation under analysis.</summary>
        public Compilation Compilation { get; }

        /// <summary>The shared compilation context.</summary>
        public SharedContext Shared { get; }

        /// <summary>Reports a diagnostic at the assembly level (no location).</summary>
        public void Report(DiagnosticDescriptor descriptor, params object[] messageArgs) =>
            Report(descriptor, Location.None, messageArgs);

        /// <summary>Reports a diagnostic at the given location.</summary>
        public void Report(DiagnosticDescriptor descriptor, Location location, params object[] messageArgs) =>
            _report(Diagnostic.Create(descriptor, location, messageArgs));
    }
}
