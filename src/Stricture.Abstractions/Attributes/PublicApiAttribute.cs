using System;

namespace Stricture
{
    /// <summary>
    /// Marks a top-level type as intentional public surface, exempting it from the
    /// internal-by-default rule.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface
        | AttributeTargets.Enum | AttributeTargets.Delegate,
        AllowMultiple = false)]
    public sealed class PublicApiAttribute : Attribute
    {
    }
}
