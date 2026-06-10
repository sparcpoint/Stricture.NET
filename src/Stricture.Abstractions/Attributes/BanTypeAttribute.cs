using System;

namespace Stricture
{
    /// <summary>Bans usage of a specific type. Provide either <see cref="Type"/> or <see cref="FullyQualifiedName"/>.</summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class BanTypeAttribute : Attribute
    {
        /// <summary>The banned type.</summary>
        public Type? Type { get; set; }

        /// <summary>The fully-qualified metadata name of the banned type.</summary>
        public string? FullyQualifiedName { get; set; }

        /// <summary>An optional message explaining the ban.</summary>
        public string? Message { get; set; }

        /// <summary>The severity this rule reports its violations at. Defaults to <see cref="Severity.Warning"/>.</summary>
        public Severity Severity { get; set; }
    }
}
