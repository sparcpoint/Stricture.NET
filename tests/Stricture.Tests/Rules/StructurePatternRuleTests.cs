using System.Threading.Tasks;
using Stricture.Rules;
using Stricture.Tests.Infrastructure;
using Xunit;

namespace Stricture.Tests.Rules
{
    /// <summary>ARCH1002 — a type under a structure root whose path does not match the pattern shape.</summary>
    public sealed class StructurePatternRuleTests
    {
        private const string Policy =
            "using Stricture;\n[assembly: FolderStructure(\"Features\", Pattern = \"{feature}/{category}\")]\n";

        [Fact]
        public void Logic_FiresWhenPathDepthDoesNotMatchPattern()
        {
            var source = Policy + "namespace N { public enum Status { A } }";
            var diags = RuleTestHarness.RunType(new StructurePatternRule(), source, "/proj/Features/Status.cs");
            Assert.Equal("ARCH1002", Assert.Single(diags).Id);
        }

        [Fact]
        public void Logic_SilentWhenPathMatchesPattern()
        {
            var source = Policy + "namespace N { public enum Status { A } }";
            var diags = RuleTestHarness.RunType(new StructurePatternRule(), source, "/proj/Features/Pay/Enumerations/Status.cs");
            Assert.Empty(diags);
        }

        [Fact]
        public async Task Integration_Fail_WrongDepth()
        {
            await StrictureAnalyzerTest.WithSource("/Features/Status.cs",
                Policy + "namespace N { public enum {|ARCH1002:Status|} { A } }").RunAsync();
        }

        [Fact]
        public async Task Integration_Pass_CorrectDepth()
        {
            await StrictureAnalyzerTest.WithSource("/Features/Pay/Misc/Status.cs",
                Policy + "namespace N { public enum Status { A } }").RunAsync();
        }
    }
}
