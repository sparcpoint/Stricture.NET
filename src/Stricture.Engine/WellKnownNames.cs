namespace Stricture
{
    /// <summary>
    /// The metadata names the engine uses to find the consumer's <c>Stricture.Abstractions</c>
    /// attributes. The engine does not reference the abstractions assembly; it matches by string.
    /// A guard test asserts these equal the real attribute type names so they cannot drift.
    /// </summary>
    internal static class WellKnownNames
    {
        /// <summary>The root namespace. Must equal the abstractions' namespace or the analyzer finds nothing.</summary>
        public const string Ns = "Stricture";

        public const string FolderStructure = Ns + ".FolderStructureAttribute";
        public const string TypeFolder = Ns + ".TypeFolderAttribute";
        public const string OneTypePerFile = Ns + ".OneTypePerFileAttribute";
        public const string CoLocateBySuffix = Ns + ".CoLocateBySuffixAttribute";
        public const string DefaultVisibility = Ns + ".DefaultVisibilityAttribute";
        public const string PublicApi = Ns + ".PublicApiAttribute";
        public const string BanType = Ns + ".BanTypeAttribute";
        public const string BanNamespace = Ns + ".BanNamespaceAttribute";
        public const string BanPackage = Ns + ".BanPackageAttribute";
    }

    /// <summary>Mirrors <c>Stricture.TypeShape</c>; values must match the abstractions enum.</summary>
    internal enum TypeShape
    {
        None = 0,
        Class,
        Struct,
        Interface,
        Enum,
        Record,
        Delegate,
    }

    /// <summary>Mirrors <c>Stricture.RuleTier</c>; values must match the abstractions enum.</summary>
    internal enum RuleTier
    {
        Auto = 0,
        Structural,
        Convention,
    }

    /// <summary>Mirrors <c>Stricture.Visibility</c>; values must match the abstractions enum.</summary>
    internal enum Visibility
    {
        Internal = 0,
        Public,
    }
}
