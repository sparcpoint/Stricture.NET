using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Stricture.Rules;
using Stricture.Tests.Infrastructure;
using Xunit;

namespace Stricture.Tests.Rules
{
    /// <summary>ARCH0001 — invalid or contradictory configuration.</summary>
    public sealed class ConfigValidationRuleTests
    {
        [Fact]
        public void Logic_FiresForPatternMissingCategory()
        {
            const string source =
                "using Stricture;\n" +
                "[assembly: FolderStructure(\"Features\", Pattern = \"{feature}\")]\n";
            var diags = RuleTestHarness.RunCompilation(new ConfigValidationRule(), source, "/proj/Architecture.cs");
            Assert.Equal("ARCH0001", Assert.Single(diags).Id);
        }

        [Fact]
        public void Logic_SilentForValidConfiguration()
        {
            const string source =
                "using Stricture;\n" +
                "[assembly: FolderStructure(\"Features\", Pattern = \"{feature}/{category}\")]\n";
            var diags = RuleTestHarness.RunCompilation(new ConfigValidationRule(), source, "/proj/Architecture.cs");
            Assert.Empty(diags);
        }

        [Fact]
        public async Task Integration_Fail_PatternMissingCategory()
        {
            var test = StrictureAnalyzerTest.WithSource("/Policy.cs",
                "using Stricture;\n[assembly: FolderStructure(\"Features\", Pattern = \"{feature}\")]\n");
            test.ExpectedDiagnostics.Add(new DiagnosticResult("ARCH0001", DiagnosticSeverity.Warning).WithNoLocation());
            await test.RunAsync();
        }

        [Fact]
        public async Task Integration_Pass_ValidConfig()
        {
            await StrictureAnalyzerTest.WithSource("/Policy.cs",
                "using Stricture;\n[assembly: FolderStructure(\"Features\", Pattern = \"{feature}/{category}\")]\n")
                .RunAsync();
        }
    }
}
