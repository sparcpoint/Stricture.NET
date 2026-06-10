using Microsoft.CodeAnalysis;

namespace Stricture.Rules
{
    /// <summary>ARCH2001: more than one top-level type in a file that is not a permitted co-location group.</summary>
    public sealed class OneTypePerFileRule : FileRule
    {
        /// <inheritdoc />
        public override DiagnosticDescriptor Descriptor => Descriptors.Arch2001;

        /// <inheritdoc />
        public override void Analyze(FileRuleContext ctx)
        {
            var policy = ctx.Shared.Policy;
            if (!policy.OneTypePerFile)
            {
                return;
            }

            var types = FileSyntax.GetTopLevelTypes(ctx.Tree.GetRoot());
            if (types.Count <= 1)
            {
                return;
            }

            if (FileSyntax.IsValidSingleGroup(types, policy.CoLocateGroups, policy.RequireSharedStem))
            {
                return;
            }

            // If every type is a co-location candidate, ARCH2002 owns the deeper diagnosis.
            var allKnown = true;
            foreach (var t in types)
            {
                if (FileSyntax.GroupIndexOf(t.Name, policy.CoLocateGroups) < 0)
                {
                    allKnown = false;
                    break;
                }
            }

            if (allKnown && policy.CoLocateGroups.Length > 0)
            {
                return;
            }

            // Report the odd-one-out: the last type that is not a co-location candidate.
            TopLevelType oddOne = types[types.Count - 1];
            for (var i = types.Count - 1; i >= 0; i--)
            {
                if (FileSyntax.GroupIndexOf(types[i].Name, policy.CoLocateGroups) < 0)
                {
                    oddOne = types[i];
                    break;
                }
            }

            ctx.Report(Descriptor, oddOne.Location, oddOne.Name);
        }
    }
}
