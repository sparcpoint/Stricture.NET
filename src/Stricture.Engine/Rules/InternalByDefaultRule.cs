using Microsoft.CodeAnalysis;

namespace Stricture.Rules
{
    /// <summary>ARCH1020: when default visibility is internal, a public top-level type lacking [PublicApi] should be internal.</summary>
    public sealed class InternalByDefaultRule : TypeRule
    {
        /// <inheritdoc />
        public override DiagnosticDescriptor Descriptor => Descriptors.Arch1020;

        /// <inheritdoc />
        public override void Analyze(TypeRuleContext ctx)
        {
            if (!ctx.Shared.Policy.DefaultVisibilityIsInternal)
            {
                return;
            }

            if (!ctx.IsTopLevel || ctx.Accessibility != Accessibility.Public)
            {
                return;
            }

            if (ctx.HasAttributeNamed(WellKnownNames.PublicApi))
            {
                return;
            }

            ctx.Report(Descriptor, ctx.TypeName);
        }
    }
}
