using System;
using System.Collections.Generic;
using Xunit;

namespace Stricture.Tests.Guards
{
    /// <summary>
    /// Name-sync guard (§7.3): the engine's metadata-name strings must equal the real Abstractions
    /// attribute type names, or the analyzer silently matches nothing.
    /// </summary>
    public sealed class NameSyncGuardTests
    {
        public static IEnumerable<object[]> NameMappings() => new[]
        {
            new object[] { WellKnownNames.FolderStructure, typeof(FolderStructureAttribute) },
            new object[] { WellKnownNames.TypeFolder, typeof(TypeFolderAttribute) },
            new object[] { WellKnownNames.OneTypePerFile, typeof(OneTypePerFileAttribute) },
            new object[] { WellKnownNames.CoLocateBySuffix, typeof(CoLocateBySuffixAttribute) },
            new object[] { WellKnownNames.DefaultVisibility, typeof(DefaultVisibilityAttribute) },
            new object[] { WellKnownNames.PublicApi, typeof(PublicApiAttribute) },
            new object[] { WellKnownNames.BanType, typeof(BanTypeAttribute) },
            new object[] { WellKnownNames.BanNamespace, typeof(BanNamespaceAttribute) },
            new object[] { WellKnownNames.BanPackage, typeof(BanPackageAttribute) },
            new object[] { WellKnownNames.ExtensionMethodHome, typeof(ExtensionMethodHomeAttribute) },
        };

        [Fact]
        public void Namespace_IsExactlyStricture()
        {
            // If this drifts, the analyzer silently matches no attributes and finds nothing.
            Assert.Equal("Stricture", WellKnownNames.Ns);
        }

        [Theory]
        [MemberData(nameof(NameMappings))]
        public void WellKnownName_MatchesAbstractionsType(string wellKnownName, Type attributeType)
        {
            Assert.Equal(attributeType.FullName, wellKnownName);
        }
    }
}
