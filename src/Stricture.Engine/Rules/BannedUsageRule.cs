using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Stricture.Rules
{
    /// <summary>ARCH3001: usage of a banned type or a type in a banned namespace.</summary>
    public sealed class BannedUsageRule : OperationRule
    {
        /// <inheritdoc />
        public override DiagnosticDescriptor Descriptor => Descriptors.Arch3001;

        /// <inheritdoc />
        public override void Analyze(OperationRuleContext ctx)
        {
            var policy = ctx.Shared.Policy;
            if (policy.BannedTypes.Length == 0 && policy.BannedNamespaces.Length == 0)
            {
                return;
            }

            var type = GetTargetType(ctx.Operation);
            if (type is null)
            {
                return;
            }

            var fullName = FullName(type);

            foreach (var ban in policy.BannedTypes)
            {
                var hit = (ban.Symbol != null
                          && SymbolEqualityComparer.Default.Equals(ban.Symbol.OriginalDefinition, type.OriginalDefinition))
                       || (!string.IsNullOrEmpty(ban.FullyQualifiedName)
                          && string.Equals(ban.FullyQualifiedName, fullName, StringComparison.Ordinal));
                if (hit)
                {
                    ctx.Report(Descriptor, ban.Severity, Detail($"type '{fullName}'", ban.Message));
                    return;
                }
            }

            var ns = type.ContainingNamespace is { IsGlobalNamespace: false } n ? n.ToDisplayString() : string.Empty;
            if (ns.Length > 0)
            {
                foreach (var ban in policy.BannedNamespaces)
                {
                    if (string.Equals(ns, ban.Namespace, StringComparison.Ordinal)
                        || ns.StartsWith(ban.Namespace + ".", StringComparison.Ordinal))
                    {
                        ctx.Report(Descriptor, ban.Severity, Detail($"namespace '{ns}' (type '{fullName}')", ban.Message));
                        return;
                    }
                }
            }
        }

        private static string Detail(string subject, string? message) =>
            string.IsNullOrEmpty(message) ? subject + " is banned." : subject + " is banned — " + message;

        private static INamedTypeSymbol? GetTargetType(IOperation op)
        {
            switch (op)
            {
                case IInvocationOperation inv:
                    return inv.TargetMethod.ContainingType;
                case IObjectCreationOperation oc:
                    return oc.Constructor?.ContainingType ?? oc.Type as INamedTypeSymbol;
                case IFieldReferenceOperation fr:
                    return fr.Field.ContainingType;
                case IPropertyReferenceOperation pr:
                    return pr.Property.ContainingType;
                case IMethodReferenceOperation mr:
                    return mr.Method.ContainingType;
                default:
                    return op.Type as INamedTypeSymbol;
            }
        }

        private static string FullName(INamedTypeSymbol type)
        {
            var ns = type.ContainingNamespace;
            if (ns is null || ns.IsGlobalNamespace)
            {
                return type.Name;
            }

            return ns.ToDisplayString() + "." + type.Name;
        }
    }
}
