namespace Stricture
{
    /// <summary>
    /// The result of locating a type's file under a declared <c>FolderStructure</c>: which structure
    /// it falls under, whether the path matches the structure's pattern shape, and the actual category
    /// folder extracted from the path (when the shape matches).
    /// </summary>
    public sealed class StructureMatch
    {
        internal StructureMatch(string root, string pattern, string? fallback, string? actualCategoryFolder, bool shapeMatches)
        {
            Root = root;
            Pattern = pattern;
            Fallback = fallback;
            ActualCategoryFolder = actualCategoryFolder;
            ShapeMatches = shapeMatches;
        }

        /// <summary>The structure root segment this type's path falls under.</summary>
        public string Root { get; }

        /// <summary>The structure's path pattern.</summary>
        public string Pattern { get; }

        /// <summary>The structure's fallback category folder, if any.</summary>
        public string? Fallback { get; }

        /// <summary>The category folder extracted from the path, or <see langword="null"/> if the shape did not match.</summary>
        public string? ActualCategoryFolder { get; }

        /// <summary>Whether the path's depth matches the structure pattern's token count.</summary>
        public bool ShapeMatches { get; }
    }
}
