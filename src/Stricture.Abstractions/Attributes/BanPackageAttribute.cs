using System;

namespace Stricture
{
    /// <summary>Bans referencing an assembly (≈ banning a package, e.g. <c>"Moq"</c>).</summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class BanPackageAttribute : Attribute
    {
        /// <summary>Creates a ban on the given assembly name.</summary>
        /// <param name="assemblyName">The banned assembly's simple name.</param>
        public BanPackageAttribute(string assemblyName)
        {
            AssemblyName = assemblyName;
        }

        /// <summary>The banned assembly's simple name.</summary>
        public string AssemblyName { get; }

        /// <summary>An optional message explaining the ban.</summary>
        public string? Message { get; set; }

        /// <summary>The severity this rule reports its violations at. Defaults to <see cref="Severity.Warning"/>.</summary>
        public Severity Severity { get; set; }
    }
}
