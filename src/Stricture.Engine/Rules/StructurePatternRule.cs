using Microsoft.CodeAnalysis;

namespace Stricture.Rules
{
    /// <summary>ARCH1002: a top-level type under a structure root whose path does not match the pattern shape.</summary>
    public sealed class StructurePatternRule : TypeRule
    {
        /// <inheritdoc />
        public override DiagnosticDescriptor Descriptor => Descriptors.Arch1002;

        /// <inheritdoc />
        public override void Analyze(TypeRuleContext ctx)
        {
            if (!ctx.IsTopLevel || ctx.FilePath is null)
            {
                return;
            }

            if (!ctx.TryGetStructure(out var structure, out _) || structure.ShapeMatches)
            {
                return;
            }

            ctx.Report(Descriptor, ctx.TypeName, structure.Pattern);
        }
    }
}
