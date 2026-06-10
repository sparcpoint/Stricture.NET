namespace Stricture
{
    /// <summary>The language shape of a type, used to classify types structurally.</summary>
    public enum TypeShape
    {
        /// <summary>No shape specified (the signal is unused).</summary>
        None = 0,

        /// <summary>A non-record <see langword="class"/>.</summary>
        Class,

        /// <summary>A non-record <see langword="struct"/>.</summary>
        Struct,

        /// <summary>An <see langword="interface"/>.</summary>
        Interface,

        /// <summary>An <see langword="enum"/>.</summary>
        Enum,

        /// <summary>A <see langword="record"/> (class or struct).</summary>
        Record,

        /// <summary>A delegate type.</summary>
        Delegate,
    }

    /// <summary>
    /// The tier a classification rule belongs to. Structural signals win over conventional ones
    /// during category resolution.
    /// </summary>
    public enum RuleTier
    {
        /// <summary>Derive the tier from the signal (see <see cref="TypeFolderAttribute"/>).</summary>
        Auto = 0,

        /// <summary>A structural signal (language kind, base type).</summary>
        Structural,

        /// <summary>A conventional signal (name suffix, marker attribute, record-ness).</summary>
        Convention,
    }

    /// <summary>The default declared visibility a project expects for its top-level types.</summary>
    public enum Visibility
    {
        /// <summary>Types are expected to be internal unless explicitly marked public surface.</summary>
        Internal = 0,

        /// <summary>Types may be public.</summary>
        Public,
    }
}
