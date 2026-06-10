namespace Stricture
{
    /// <summary>The default declared visibility a project expects for its top-level types.</summary>
    public enum Visibility
    {
        /// <summary>Types are expected to be internal unless explicitly marked public surface.</summary>
        Internal = 0,

        /// <summary>Types may be public.</summary>
        Public,
    }
}
