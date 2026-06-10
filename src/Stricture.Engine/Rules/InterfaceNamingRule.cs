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
}
