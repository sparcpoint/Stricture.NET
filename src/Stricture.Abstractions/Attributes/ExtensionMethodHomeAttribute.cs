using System;

namespace Stricture
{
    /// <summary>
    /// Requires that every extension method for <see cref="ExtendedType"/> be declared in a single,
    /// consistently named host class — optionally <c>partial</c> and in a fixed namespace. Keeps a
    /// family of extensions (e.g. <c>IServiceCollection</c> registrations) discoverable in one place.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class ExtensionMethodHomeAttribute : Attribute
    {
        /// <summary>Creates a convention for extension methods on <paramref name="extendedType"/>.</summary>
        /// <param name="extendedType">
        /// The type whose extension methods this governs, e.g. <c>typeof(IServiceCollection)</c>.
        /// Matched against the extension method's <c>this</c> parameter type exactly.
        /// </param>
        /// <param name="className">The required name of the host class, e.g. <c>"ServiceCollectionExtensions"</c>.</param>
        public ExtensionMethodHomeAttribute(Type extendedType, string className)
        {
            ExtendedType = extendedType;
            ClassName = className;
        }

        /// <summary>The type whose extension methods this convention governs.</summary>
        public Type ExtendedType { get; }

        /// <summary>The required name of the host class.</summary>
        public string ClassName { get; }

        /// <summary>The namespace the host class must live in. When <see langword="null"/>, the namespace is not enforced.</summary>
        public string? Namespace { get; set; }

        /// <summary>Whether the host class must be declared <c>partial</c>. Defaults to <see langword="true"/>.</summary>
        public bool MustBePartial { get; set; } = true;

        /// <summary>The severity this rule reports its violations at. Defaults to <see cref="Severity.Warning"/>.</summary>
        public Severity Severity { get; set; }
    }
}
