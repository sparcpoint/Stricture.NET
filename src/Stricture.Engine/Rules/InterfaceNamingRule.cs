using System;
using Microsoft.CodeAnalysis;

namespace Stricture.Rules
{
    /// <summary>ARCH1010: a concrete type named after the interface it implements (X implementing IX).</summary>
    public sealed class InterfaceNamingRule : TypeRule
    {
        /// <inheritdoc />
        public override DiagnosticDescriptor Descriptor => Descriptors.Arch1010;

        /// <inheritdoc />
        public override void Analyze(TypeRuleContext ctx)
        {
            // Opt-in: the rule is inert unless [assembly: ForbidInterfaceNaming] is present.
            if (ctx.Shared.Policy.InterfaceNamingSeverity is not { } severity)
            {
                return;
            }

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
                    ctx.Report(Descriptor, severity, ctx.TypeName, iface.Name);
                    return;
                }
            }
        }
    }
}
