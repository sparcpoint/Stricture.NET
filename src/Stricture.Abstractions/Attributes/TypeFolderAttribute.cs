using System;

namespace Stricture
{
    /// <summary>Maps a classification signal to a category folder.</summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class TypeFolderAttribute : Attribute
    {
        /// <summary>Creates a mapping that places matching types in <paramref name="folder"/>.</summary>
        /// <param name="folder">The category folder matching types belong in.</param>
        public TypeFolderAttribute(string folder)
        {
            Folder = folder;
        }

        /// <summary>The category folder matching types belong in.</summary>
        public string Folder { get; }

        /// <summary>Match types of this language shape. Structural signal.</summary>
        public TypeShape Kind { get; set; }

        /// <summary>Match types deriving from this base type. Structural signal.</summary>
        public Type? DerivesFrom { get; set; }

        /// <summary>Match types implementing this interface. Conventional signal.</summary>
        public Type? Implements { get; set; }

        /// <summary>Match types annotated with this attribute. Conventional signal.</summary>
        public Type? MarkedWith { get; set; }

        /// <summary>Match types whose name ends with this suffix. Conventional signal.</summary>
        public string? EndsWith { get; set; }

        /// <summary>Match record types. Conventional signal.</summary>
        public bool IsRecord { get; set; }

        /// <summary>
        /// The tier this mapping belongs to. When <see cref="RuleTier.Auto"/>, the tier is derived:
        /// <see cref="Kind"/>/<see cref="DerivesFrom"/> are structural; the rest are conventional.
        /// </summary>
        public RuleTier Tier { get; set; } = RuleTier.Auto;
    }
}
