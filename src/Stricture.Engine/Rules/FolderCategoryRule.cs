using System;
using Microsoft.CodeAnalysis;

namespace Stricture.Rules
{
    /// <summary>ARCH1001: a top-level type sits in the wrong category folder.</summary>
    public sealed class FolderCategoryRule : TypeRule
    {
        /// <inheritdoc />
        public override DiagnosticDescriptor Descriptor => Descriptors.Arch1001;

        /// <inheritdoc />
        public override void Analyze(TypeRuleContext ctx)
        {
            if (!ctx.IsTopLevel || ctx.FilePath is null)
            {
                return;
            }

            if (!ctx.TryGetStructure(out var structure, out var actual) || !structure.ShapeMatches)
            {
                return;
            }

            var match = ctx.ResolveCategoryMatch();
            var expected = match?.Folder ?? structure.Fallback;
            if (expected is null)
            {
                return;
            }

            if (!string.Equals(actual, expected, StringComparison.Ordinal))
            {
                var severity = match?.Severity ?? structure.Severity;
                ctx.Report(Descriptor, severity, ctx.TypeName, expected, actual ?? string.Empty);
            }
        }
    }
}
