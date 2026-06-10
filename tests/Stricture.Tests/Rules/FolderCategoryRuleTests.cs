using System.Threading.Tasks;
using Stricture.Rules;
using Stricture.Tests.Infrastructure;
using Xunit;

namespace Stricture.Tests.Rules
{
    /// <summary>ARCH1001 — a top-level type sits in the wrong category folder.</summary>
    public sealed class FolderCategoryRuleTests
    {
        private const string Policy =
            "using Stricture;\n" +
            "[assembly: FolderStructure(\"Features\", Pattern = \"{feature}/{category}\", Fallback = \"Models\")]\n" +
            "[assembly: TypeFolder(\"Enumerations\", Kind = TypeShape.Enum)]\n";

        [Fact]
        public void Logic_FiresWhenTypeIsInWrongCategoryFolder()
        {
            var source = Policy + "namespace N { public enum Status { A } }";
            var diags = RuleTestHarness.RunType(new FolderCategoryRule(), source, "/proj/Features/Pay/Models/Status.cs");
            Assert.Equal("ARCH1001", Assert.Single(diags).Id);
        }

        [Fact]
        public void Logic_SilentWhenTypeIsInCorrectFolder()
        {
            var source = Policy + "namespace N { public enum Status { A } }";
            var diags = RuleTestHarness.RunType(new FolderCategoryRule(), source, "/proj/Features/Pay/Enumerations/Status.cs");
            Assert.Empty(diags);
        }

        [Fact]
        public void Logic_UsesFallbackWhenTypeIsUnclassified()
        {
            // A plain class classifies to nothing, so it is expected in the structure's Fallback ("Models").
            var source = Policy + "namespace N { public class Widget { } }";
            var inFallback = RuleTestHarness.RunType(new FolderCategoryRule(), source, "/proj/Features/Pay/Models/Widget.cs");
            Assert.Empty(inFallback);

            var elsewhere = RuleTestHarness.RunType(new FolderCategoryRule(), source, "/proj/Features/Pay/Services/Widget.cs");
            Assert.Equal("ARCH1001", Assert.Single(elsewhere).Id);
        }

        [Fact]
        public async Task Integration_Fail_WrongFolder()
        {
            await StrictureAnalyzerTest.WithSource("/Features/Pay/Models/Status.cs",
                Policy + "namespace N { public enum {|ARCH1001:Status|} { A } }").RunAsync();
        }

        [Fact]
        public async Task Integration_Pass_CorrectFolder()
        {
            await StrictureAnalyzerTest.WithSource("/Features/Pay/Enumerations/Status.cs",
                Policy + "namespace N { public enum Status { A } }").RunAsync();
        }
    }
}
