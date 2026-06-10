using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Stricture.Rules
{
    /// <summary>ARCH0001: invalid or contradictory configuration, reported once per issue.</summary>
    public sealed class ConfigValidationRule : CompilationRule
    {
        /// <inheritdoc />
        public override IEnumerable<DiagnosticDescriptor> Descriptors => new[] { Stricture.Descriptors.Arch0001 };

        /// <inheritdoc />
        public override void Analyze(CompilationRuleContext ctx)
        {
            foreach (var issue in ctx.Shared.Policy.ConfigIssues)
            {
                ctx.Report(Stricture.Descriptors.Arch0001, issue);
            }
        }
    }
}
