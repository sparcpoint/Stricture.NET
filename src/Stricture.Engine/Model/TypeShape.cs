namespace Stricture
{
    /// <summary>Mirrors <c>Stricture.TypeShape</c>; values must match the abstractions enum.</summary>
    internal enum TypeShape
    {
        None = 0,
        Class,
        Struct,
        Interface,
        Enum,
        Record,
        Delegate,
    }
}
