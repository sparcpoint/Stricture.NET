using System;

namespace Stricture
{
    /// <summary>
    /// Declares a structure root and a path template under it. <c>{category}</c> is the slot that
    /// classification rules map into; other <c>{tokens}</c> match any single path segment.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class FolderStructureAttribute : Attribute
    {
        /// <summary>Creates a structure rooted at <paramref name="root"/>.</summary>
        /// <param name="root">The root folder (relative to the project) this structure governs.</param>
        public FolderStructureAttribute(string root)
        {
            Root = root;
        }

        /// <summary>The root folder this structure governs.</summary>
        public string Root { get; }

        /// <summary>The path template, e.g. <c>"{feature}/{category}"</c>. Must contain <c>{category}</c>.</summary>
        public string Pattern { get; set; } = string.Empty;

        /// <summary>The category folder used for types this structure cannot otherwise classify.</summary>
        public string? Fallback { get; set; }
    }
}
