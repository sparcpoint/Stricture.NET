namespace Stricture
{
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
}
