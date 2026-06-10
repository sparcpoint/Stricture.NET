using Microsoft.CodeAnalysis;

namespace Stricture
{
    /// <summary>
    /// A declared <c>ExtensionMethodHome</c>: the required host class for extension methods on a
    /// given type.
    /// </summary>
    internal sealed class ExtensionHomePolicy
    {
        public ExtensionHomePolicy(INamedTypeSymbol? extendedType, string className, string? @namespace, bool mustBePartial)
        {
            ExtendedType = extendedType;
            ClassName = className;
            Namespace = @namespace;
            MustBePartial = mustBePartial;
        }

        /// <summary>The type whose extension methods this governs, or <see langword="null"/> if it could not be resolved.</summary>
        public INamedTypeSymbol? ExtendedType { get; }

        /// <summary>The required host class name.</summary>
        public string ClassName { get; }

        /// <summary>The required host namespace, or <see langword="null"/> if not enforced.</summary>
        public string? Namespace { get; }

        /// <summary>Whether the host class must be declared <c>partial</c>.</summary>
        public bool MustBePartial { get; }

        /// <summary>True when the policy is usable: the extended type resolved and a class name was given.</summary>
        public bool IsActive => ExtendedType != null && !string.IsNullOrEmpty(ClassName);

        /// <summary>
        /// True when <paramref name="type"/> declares at least one extension method whose <c>this</c>
        /// parameter type is exactly <see cref="ExtendedType"/>.
        /// </summary>
        public bool HostsExtensionFor(INamedTypeSymbol type)
        {
            if (ExtendedType is null)
            {
                return false;
            }

            foreach (var member in type.GetMembers())
            {
                if (member is IMethodSymbol method
                    && method.IsExtensionMethod
                    && method.Parameters.Length > 0
                    && SymbolEqualityComparer.Default.Equals(
                        method.Parameters[0].Type.OriginalDefinition, ExtendedType.OriginalDefinition))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
