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
}
