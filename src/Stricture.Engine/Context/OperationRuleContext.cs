using System;
using Microsoft.CodeAnalysis;

namespace Stricture
{
    /// <summary>The per-operation context.</summary>
    public sealed class OperationRuleContext
    {
        private readonly Action<Diagnostic> _report;

        /// <summary>Creates a context for one operation.</summary>
        public OperationRuleContext(IOperation operation, SharedContext shared, Action<Diagnostic> report)
        {
            Operation = operation ?? throw new ArgumentNullException(nameof(operation));
            Shared = shared ?? throw new ArgumentNullException(nameof(shared));
            _report = report ?? throw new ArgumentNullException(nameof(report));
        }

        /// <summary>The operation under analysis.</summary>
        public IOperation Operation { get; }

        /// <summary>The shared compilation context.</summary>
        public SharedContext Shared { get; }

        /// <summary>Reports a diagnostic on the operation's syntax.</summary>
        public void Report(DiagnosticDescriptor descriptor, params object[] messageArgs) =>
            Report(descriptor, Operation.Syntax.GetLocation(), messageArgs);

        /// <summary>Reports a diagnostic at the given location.</summary>
        public void Report(DiagnosticDescriptor descriptor, Location location, params object[] messageArgs) =>
            _report(Diagnostic.Create(descriptor, location, messageArgs));

        /// <summary>Reports a diagnostic on the operation's syntax at the given severity.</summary>
        public void Report(DiagnosticDescriptor descriptor, DiagnosticSeverity severity, params object[] messageArgs) =>
            Report(descriptor, severity, Operation.Syntax.GetLocation(), messageArgs);

        /// <summary>Reports a diagnostic at the given location and severity.</summary>
        public void Report(DiagnosticDescriptor descriptor, DiagnosticSeverity severity, Location location, params object[] messageArgs) =>
            _report(DiagnosticFactory.Create(descriptor, severity, location, messageArgs));
    }
}
