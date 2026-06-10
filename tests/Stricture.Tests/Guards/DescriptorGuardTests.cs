using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Stricture.Tests.Guards
{
    /// <summary>
    /// Descriptor guard (§7.3): supported diagnostic ids are unique and every one is listed in the
    /// unshipped analyzer-release tracking file.
    /// </summary>
    public sealed class DescriptorGuardTests
    {
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
            var asm = typeof(DescriptorGuardTests).Assembly;
            using var stream = asm.GetManifestResourceStream("AnalyzerReleases.Unshipped.md")
                ?? throw new InvalidOperationException("Embedded AnalyzerReleases.Unshipped.md not found.");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
