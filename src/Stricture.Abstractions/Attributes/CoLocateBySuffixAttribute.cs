using System;

namespace Stricture
{
    /// <summary>
    /// Declares a group of name suffixes whose types may legally share a file (e.g.
    /// <c>"Request"</c> and <c>"Response"</c>).
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class CoLocateBySuffixAttribute : Attribute
    {
        /// <summary>Creates a co-location group from the given suffixes.</summary>
        /// <param name="suffixes">The suffixes whose types may share a file.</param>
        public CoLocateBySuffixAttribute(params string[] suffixes)
        {
            Suffixes = suffixes ?? Array.Empty<string>();
        }

        /// <summary>The suffixes whose types may share a file.</summary>
        public string[] Suffixes { get; }
    }
}
