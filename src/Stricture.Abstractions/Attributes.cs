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

    /// <summary>Turns on the one-top-level-type-per-file rule.</summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class OneTypePerFileAttribute : Attribute
    {
        /// <summary>
        /// When <see langword="true"/>, co-located types must share a common stem (e.g. <c>Foo</c> in
        /// <c>FooRequest</c>/<c>FooResponse</c>).
        /// </summary>
        public bool RequireSharedStem { get; set; } = true;
    }

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

    /// <summary>
    /// Sets the expected default visibility for top-level types. When set to
    /// <see cref="Visibility.Internal"/>, enables the internal-by-default rule.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class DefaultVisibilityAttribute : Attribute
    {
        /// <summary>Creates the rule with the given expected default visibility.</summary>
        /// <param name="visibility">The expected default visibility.</param>
        public DefaultVisibilityAttribute(Visibility visibility)
        {
            Visibility = visibility;
        }

        /// <summary>The expected default visibility.</summary>
        public Visibility Visibility { get; }
    }

    /// <summary>
    /// Marks a top-level type as intentional public surface, exempting it from the
    /// internal-by-default rule.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface
        | AttributeTargets.Enum | AttributeTargets.Delegate,
        AllowMultiple = false)]
    public sealed class PublicApiAttribute : Attribute
    {
    }

    /// <summary>Bans usage of a specific type. Provide either <see cref="Type"/> or <see cref="FullyQualifiedName"/>.</summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class BanTypeAttribute : Attribute
    {
        /// <summary>The banned type.</summary>
        public Type? Type { get; set; }

        /// <summary>The fully-qualified metadata name of the banned type.</summary>
        public string? FullyQualifiedName { get; set; }

        /// <summary>An optional message explaining the ban.</summary>
        public string? Message { get; set; }
    }

    /// <summary>Bans usage of any type in a namespace (and its sub-namespaces).</summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class BanNamespaceAttribute : Attribute
    {
        /// <summary>Creates a ban on the given namespace.</summary>
        /// <param name="namespace">The banned namespace.</param>
        public BanNamespaceAttribute(string @namespace)
        {
            Namespace = @namespace;
        }

        /// <summary>The banned namespace.</summary>
        public string Namespace { get; }

        /// <summary>An optional message explaining the ban.</summary>
        public string? Message { get; set; }
    }

    /// <summary>Bans referencing an assembly (≈ banning a package, e.g. <c>"Moq"</c>).</summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class BanPackageAttribute : Attribute
    {
        /// <summary>Creates a ban on the given assembly name.</summary>
        /// <param name="assemblyName">The banned assembly's simple name.</param>
        public BanPackageAttribute(string assemblyName)
        {
            AssemblyName = assemblyName;
        }

        /// <summary>The banned assembly's simple name.</summary>
        public string AssemblyName { get; }

        /// <summary>An optional message explaining the ban.</summary>
        public string? Message { get; set; }
    }
}
