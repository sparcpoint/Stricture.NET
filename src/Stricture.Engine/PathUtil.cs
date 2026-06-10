using System;
using System.Collections.Generic;

namespace Stricture
{
    /// <summary>
    /// Path/segment helpers written to avoid <c>netstandard2.0</c>-unavailable APIs
    /// (no <c>Path.GetRelativePath</c>, no <c>string.Contains(char)</c>).
    /// </summary>
    internal static class PathUtil
    {
        /// <summary>Normalizes back-slashes to forward-slashes.</summary>
        public static string NormalizeSlashes(string path) =>
            path is null ? string.Empty : path.Replace('\\', '/');

        /// <summary>Returns true if <paramref name="value"/> contains <paramref name="substring"/> (ordinal).</summary>
        public static bool ContainsOrdinal(string value, string substring) =>
            value != null && value.IndexOf(substring, StringComparison.Ordinal) >= 0;

        /// <summary>
        /// Returns the directory segments of a file path (folders only, excluding the file name),
        /// with empty segments removed. Paths are normalized to forward slashes first.
        /// </summary>
        public static IReadOnlyList<string> GetDirectorySegments(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return Array.Empty<string>();
            }

            var normalized = NormalizeSlashes(filePath!);
            var lastSlash = normalized.LastIndexOf('/');
            if (lastSlash < 0)
            {
                return Array.Empty<string>();
            }

            var dir = normalized.Substring(0, lastSlash);
            var parts = dir.Split('/');
            var result = new List<string>(parts.Length);
            foreach (var p in parts)
            {
                if (p.Length > 0)
                {
                    result.Add(p);
                }
            }

            return result;
        }

        /// <summary>Returns the leaf directory name of a file path, or <see langword="null"/>.</summary>
        public static string? GetLeafFolder(string? filePath)
        {
            var segments = GetDirectorySegments(filePath);
            return segments.Count == 0 ? null : segments[segments.Count - 1];
        }

        /// <summary>Returns the file name (last segment) of a path.</summary>
        public static string GetFileName(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return string.Empty;
            }

            var normalized = NormalizeSlashes(filePath!);
            var lastSlash = normalized.LastIndexOf('/');
            return lastSlash < 0 ? normalized : normalized.Substring(lastSlash + 1);
        }
    }
}
