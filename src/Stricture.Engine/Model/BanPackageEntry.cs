using Microsoft.CodeAnalysis;

namespace Stricture
{
    /// <summary>A declared <c>BanPackage</c>.</summary>
    internal sealed class BanPackageEntry
    {
        public BanPackageEntry(string assemblyName, string? message, DiagnosticSeverity severity)
        {
            AssemblyName = assemblyName;
            Message = message;
            Severity = severity;
        }

        public string AssemblyName { get; }

        public string? Message { get; }

        /// <summary>The severity violations of this ban are reported at.</summary>
        public DiagnosticSeverity Severity { get; }
    }
}
