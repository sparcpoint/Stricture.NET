using System;

namespace Stricture
{
    /// <summary>Bans usage of any type in a namespace (and its sub-namespaces).</summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class BanNamespaceAttribute : Attribute
    {
        /// <summary>Creates a ban on the given namespace.</summary>
        /// <param name="namespace">The banned namespace.</param>
        public BanNamespaceAttribute(string @namespace)
        {
            Namespace = @namespace;
        }

        /// <summary>The banned namespace.</summary>
        public string Namespace { get; }

        /// <summary>An optional message explaining the ban.</summary>
        public string? Message { get; set; }
    }
}
