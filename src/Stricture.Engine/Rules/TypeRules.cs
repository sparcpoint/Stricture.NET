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

            var expected = ctx.ResolveCategory() ?? structure.Fallback;
            if (expected is null)
            {
                return;
            }

            if (!string.Equals(actual, expected, StringComparison.Ordinal))
            {
                ctx.Report(Descriptor, ctx.TypeName, expected, actual ?? string.Empty);
            }
        }
    }

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

    /// <summary>ARCH1010: a concrete type named after the interface it implements (X implementing IX).</summary>
    public sealed class InterfaceNamingRule : TypeRule
    {
        /// <inheritdoc />
        public override DiagnosticDescriptor Descriptor => Descriptors.Arch1010;

        /// <inheritdoc />
        public override void Analyze(TypeRuleContext ctx)
        {
            var type = ctx.Type;
            if (type.TypeKind != TypeKind.Class && type.TypeKind != TypeKind.Struct)
            {
                return;
            }

            if (type.IsAbstract)
            {
                return;
            }

            var expectedInterfaceName = "I" + type.Name;
            foreach (var iface in type.Interfaces)
            {
                if (string.Equals(iface.Name, expectedInterfaceName, StringComparison.Ordinal))
                {
                    ctx.Report(Descriptor, ctx.TypeName, iface.Name);
                    return;
                }
            }
        }
    }

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
