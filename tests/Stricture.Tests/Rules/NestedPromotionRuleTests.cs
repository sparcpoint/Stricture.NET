using System.Threading.Tasks;
using Stricture.Rules;
using Stricture.Tests.Infrastructure;
using Xunit;

namespace Stricture.Tests.Rules
{
    /// <summary>ARCH1003 — a public/internal nested type that classifies should be promoted to its own file.</summary>
    public sealed class NestedPromotionRuleTests
    {
        private const string Policy =
            "using Stricture;\n" +
            "[assembly: FolderStructure(\"Features\", Pattern = \"{feature}/{category}\", Fallback = \"Models\")]\n" +
            "[assembly: TypeFolder(\"Enumerations\", Kind = TypeShape.Enum)]\n";

        [Fact]
        public void Logic_FiresForPublicNestedClassifyingType()
        {
            var source = Policy + "namespace N { public class Outer { public enum Inner { A } } }";
            var diags = RuleTestHarness.RunType(new NestedPromotionRule(), source, "/proj/Features/Pay/Enumerations/Outer.cs");
            Assert.Equal("ARCH1003", Assert.Single(diags).Id);
        }

        [Fact]
        public void Logic_SilentForPrivateNestedType()
        {
            var source = Policy + "namespace N { public class Outer { private enum Inner { A } } }";
            var diags = RuleTestHarness.RunType(new NestedPromotionRule(), source, "/proj/Features/Pay/Enumerations/Outer.cs");
            Assert.Empty(diags);
        }

        [Fact]
        public async Task Integration_Fail_PublicNestedClassifyingType()
        {
            await StrictureAnalyzerTest.WithSource("/Other/Outer.cs",
                Policy + "namespace N { public class Outer { public enum {|ARCH1003:Inner|} { A } } }").RunAsync();
        }

        [Fact]
        public async Task Integration_Pass_PrivateNestedType()
        {
            await StrictureAnalyzerTest.WithSource("/Other/Outer.cs",
                Policy + "namespace N { public class Outer { private enum Inner { A } } }").RunAsync();
        }
    }
}
