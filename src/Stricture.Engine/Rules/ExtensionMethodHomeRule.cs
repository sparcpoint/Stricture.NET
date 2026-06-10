using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Stricture.Rules
{
    /// <summary>
    /// ARCH1030: a type that declares extension methods for a governed type must be the designated
    /// host class — the required name, (optionally) the required namespace, and (optionally) <c>partial</c>.
    /// </summary>
    public sealed class ExtensionMethodHomeRule : TypeRule
    {
        /// <inheritdoc />
        public override DiagnosticDescriptor Descriptor => Descriptors.Arch1030;

        /// <inheritdoc />
        public override void Analyze(TypeRuleContext ctx)
        {
            var homes = ctx.Shared.Policy.ExtensionHomes;
            if (homes.IsDefaultOrEmpty)
            {
                return;
            }

            foreach (var home in homes)
            {
                if (!home.IsActive || !home.HostsExtensionFor(ctx.Type))
                {
                    continue;
                }

                var nameOk = string.Equals(ctx.Type.Name, home.ClassName, StringComparison.Ordinal);
                var namespaceOk = home.Namespace is null
                    || string.Equals(ctx.Type.ContainingNamespace.ToDisplayString(), home.Namespace, StringComparison.Ordinal);
                var partialOk = !home.MustBePartial || IsDeclaredPartial(ctx.Type);

                if (nameOk && namespaceOk && partialOk)
                {
                    continue;
                }

                ctx.Report(Descriptor, ctx.TypeName, home.ExtendedType!.Name, DescribeExpectation(home));
            }
        }

        private static bool IsDeclaredPartial(INamedTypeSymbol type)
        {
            foreach (var reference in type.DeclaringSyntaxReferences)
            {
                if (reference.GetSyntax() is TypeDeclarationSyntax declaration
                    && declaration.Modifiers.Any(SyntaxKind.PartialKeyword))
                {
                    return true;
                }
            }

            return false;
        }

        private static string DescribeExpectation(ExtensionHomePolicy home)
        {
            var builder = new StringBuilder("a ");
            if (home.MustBePartial)
            {
                builder.Append("partial ");
            }

            builder.Append("class named '").Append(home.ClassName).Append('\'');
            if (home.Namespace is not null)
            {
                builder.Append(" in namespace '").Append(home.Namespace).Append('\'');
            }

            return builder.ToString();
        }
    }
}
