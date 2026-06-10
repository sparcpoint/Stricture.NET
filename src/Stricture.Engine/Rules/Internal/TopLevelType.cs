using Microsoft.CodeAnalysis;

namespace Stricture.Rules
{
    /// <summary>A top-level type declaration discovered syntactically.</summary>
    internal readonly struct TopLevelType
    {
        public TopLevelType(string name, Location location)
        {
            Name = name;
            Location = location;
        }

        public string Name { get; }

        public Location Location { get; }
    }
}
