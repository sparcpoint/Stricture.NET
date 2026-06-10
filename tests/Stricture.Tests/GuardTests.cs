using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Stricture.Tests
{
    /// <summary>The guard tests required by §7.3: name-sync and descriptor uniqueness/coverage.</summary>
    public sealed class GuardTests
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

        [Fact]
        public void SupportedDiagnostics_HaveUniqueIds()
        {
            var ids = new StrictureAnalyzer().SupportedDiagnostics.Select(d => d.Id).ToList();
            Assert.Equal(ids.Count, ids.Distinct(StringComparer.Ordinal).Count());
        }

        [Fact]
        public void EverySupportedDiagnostic_IsListedInUnshippedReleases()
        {
            var unshipped = ReadUnshipped();
            foreach (var id in new StrictureAnalyzer().SupportedDiagnostics.Select(d => d.Id))
            {
                Assert.Contains(id, unshipped, StringComparison.Ordinal);
            }
        }

        private static string ReadUnshipped()
        {
            var asm = typeof(GuardTests).Assembly;
            using var stream = asm.GetManifestResourceStream("AnalyzerReleases.Unshipped.md")
                ?? throw new InvalidOperationException("Embedded AnalyzerReleases.Unshipped.md not found.");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
