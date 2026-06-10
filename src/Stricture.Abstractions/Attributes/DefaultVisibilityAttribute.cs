using System;

namespace Stricture
{
    /// <summary>
    /// Sets the expected default visibility for top-level types. When set to
    /// <see cref="Visibility.Internal"/>, enables the internal-by-default rule.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class DefaultVisibilityAttribute : Attribute
    {
        /// <summary>Creates the rule with the given expected default visibility.</summary>
        /// <param name="visibility">The expected default visibility.</param>
        public DefaultVisibilityAttribute(Visibility visibility)
        {
            Visibility = visibility;
        }

        /// <summary>The expected default visibility.</summary>
        public Visibility Visibility { get; }

        /// <summary>The severity this rule reports its violations at. Defaults to <see cref="Severity.Warning"/>.</summary>
        public Severity Severity { get; set; }
    }
}
