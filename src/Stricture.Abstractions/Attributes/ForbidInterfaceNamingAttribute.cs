using System;

namespace Stricture
{
    /// <summary>
    /// Turns on the rule that forbids a concrete type being named after the interface it implements
    /// (e.g. a class <c>Foo</c> implementing <c>IFoo</c>); such types must take an intent-revealing name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class ForbidInterfaceNamingAttribute : Attribute
    {
        /// <summary>The severity this rule reports its violations at. Defaults to <see cref="Severity.Warning"/>.</summary>
        public Severity Severity { get; set; }
    }
}
