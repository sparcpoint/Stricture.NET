using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Stricture.Rules
{
    /// <summary>ARCH2002: co-located types mix suffix groups or (when required) have mismatched stems.</summary>
    public sealed class CoLocationRule : FileRule
    {
        /// <inheritdoc />
        public override DiagnosticDescriptor Descriptor => Descriptors.Arch2002;

        /// <inheritdoc />
        public override void Analyze(FileRuleContext ctx)
        {
            var policy = ctx.Shared.Policy;
            if (policy.CoLocateGroups.Length == 0)
            {
                return;
            }

            var types = FileSyntax.GetTopLevelTypes(ctx.Tree.GetRoot());
            if (types.Count <= 1)
            {
                return;
            }

            // Only diagnose when every type is a co-location candidate (otherwise ARCH2001 owns it).
            var distinctGroups = new HashSet<int>();
            foreach (var t in types)
            {
                var gi = FileSyntax.GroupIndexOf(t.Name, policy.CoLocateGroups);
                if (gi < 0)
                {
                    return;
                }

                distinctGroups.Add(gi);
            }

            if (FileSyntax.IsValidSingleGroup(types, policy.CoLocateGroups, policy.RequireSharedStem))
            {
                return;
            }

            string detail;
            if (distinctGroups.Count > 1)
            {
                detail = "types belong to different co-location groups.";
            }
            else
            {
                detail = "co-located types must share a common stem.";
            }

            var severity = DiagnosticSeverity.Warning;
            foreach (var gi in distinctGroups)
            {
                if (policy.CoLocateGroups[gi].Severity == DiagnosticSeverity.Error)
                {
                    severity = DiagnosticSeverity.Error;
                    break;
                }
            }

            ctx.Report(Descriptor, severity, types[0].Location, detail);
        }
    }
}
