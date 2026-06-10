using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Stricture.Rules
{
    /// <summary>ARCH4001: the compilation references a banned assembly.</summary>
    public sealed class BannedReferenceRule : CompilationRule
    {
        /// <inheritdoc />
        public override IEnumerable<DiagnosticDescriptor> Descriptors => new[] { Stricture.Descriptors.Arch4001 };

        /// <inheritdoc />
        public override void Analyze(CompilationRuleContext ctx)
        {
            var bans = ctx.Shared.Policy.BannedPackages;
            if (bans.Length == 0)
            {
                return;
            }

            foreach (var reference in ctx.Compilation.ReferencedAssemblyNames)
            {
                foreach (var ban in bans)
                {
                    if (string.Equals(reference.Name, ban.AssemblyName, StringComparison.OrdinalIgnoreCase))
                    {
                        var detail = string.IsNullOrEmpty(ban.Message)
                            ? $"'{ban.AssemblyName}'"
                            : $"'{ban.AssemblyName}' — {ban.Message}";
                        ctx.Report(Stricture.Descriptors.Arch4001, ban.Severity, detail);
                    }
                }
            }
        }
    }
}
