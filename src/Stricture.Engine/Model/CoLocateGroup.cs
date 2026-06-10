using System.Collections.Immutable;

namespace Stricture
{
    /// <summary>A declared <c>CoLocateBySuffix</c> group.</summary>
    internal sealed class CoLocateGroup
    {
        public CoLocateGroup(ImmutableArray<string> suffixes)
        {
            Suffixes = suffixes;
        }

        public ImmutableArray<string> Suffixes { get; }
    }
}
