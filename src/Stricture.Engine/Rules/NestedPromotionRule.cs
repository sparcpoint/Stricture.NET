using Microsoft.CodeAnalysis;

namespace Stricture.Rules
{
    /// <summary>ARCH1003: a public/internal nested type that classifies to a category should be promoted to its own file.</summary>
    public sealed class NestedPromotionRule : TypeRule
    {
        /// <inheritdoc />
        public override DiagnosticDescriptor Descriptor => Descriptors.Arch1003;

        /// <inheritdoc />
        public override void Analyze(TypeRuleContext ctx)
        {
            if (!ctx.IsNested)
            {
                return;
            }

            if (ctx.Accessibility != Accessibility.Public && ctx.Accessibility != Accessibility.Internal)
            {
                return;
            }

            var category = ctx.ResolveCategory();
            if (category is null)
            {
                return;
            }

            ctx.Report(Descriptor, ctx.TypeName, ctx.KindWord, category);
        }
    }
}
