using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Stricture.Rules;
using Stricture.Tests.Infrastructure;
using Xunit;

namespace Stricture.Tests.Rules
{
    /// <summary>ARCH4001 — the compilation references a banned assembly.</summary>
    public sealed class BannedReferenceRuleTests
    {
        [Fact]
        public void Logic_FiresForBannedReferencedAssembly()
        {
            const string source =
                "using Stricture;\n[assembly: BanPackage(\"Stricture.Abstractions\")]\n";
            var diags = RuleTestHarness.RunCompilation(new BannedReferenceRule(), source, "/proj/Architecture.cs");
            Assert.Equal("ARCH4001", Assert.Single(diags).Id);
        }

        [Fact]
        public void Logic_SilentWhenBannedAssemblyNotReferenced()
        {
            const string source =
                "using Stricture;\n[assembly: BanPackage(\"NoSuchAssembly\")]\n";
            var diags = RuleTestHarness.RunCompilation(new BannedReferenceRule(), source, "/proj/Architecture.cs");
            Assert.Empty(diags);
        }

        [Fact]
        public async Task Integration_Fail_BannedReference()
        {
            var test = StrictureAnalyzerTest.WithSource("/Policy.cs",
                "using Stricture;\n[assembly: BanPackage(\"Stricture.Abstractions\")]\n");
            test.ExpectedDiagnostics.Add(new DiagnosticResult("ARCH4001", DiagnosticSeverity.Warning).WithNoLocation());
            await test.RunAsync();
        }

        [Fact]
        public async Task Integration_Pass_NoBannedReference()
        {
            await StrictureAnalyzerTest.WithSource("/Policy.cs",
                "using Stricture;\n[assembly: BanPackage(\"NoSuchAssembly\")]\n").RunAsync();
        }
    }
}
