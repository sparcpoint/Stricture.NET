using System.Threading.Tasks;
using Stricture.Rules;
using Stricture.Tests.Infrastructure;
using Xunit;

namespace Stricture.Tests.Rules
{
    /// <summary>ARCH2001 — more than one top-level type per file that is not a permitted co-location group.</summary>
    public sealed class OneTypePerFileRuleTests
    {
        [Fact]
        public void Logic_FiresForMultipleUnrelatedTopLevelTypes()
        {
            const string source =
                "using Stricture;\n" +
                "[assembly: OneTypePerFile]\n" +
                "namespace N { public class Foo { } public class Bar { } }";
            var diags = RuleTestHarness.RunFile(new OneTypePerFileRule(), source, "/proj/Foo.cs");
            Assert.Equal("ARCH2001", Assert.Single(diags).Id);
        }

        [Fact]
        public void Logic_SilentForSingleTopLevelType()
        {
            const string source =
                "using Stricture;\n" +
                "[assembly: OneTypePerFile]\n" +
                "namespace N { public class Foo { } }";
            var diags = RuleTestHarness.RunFile(new OneTypePerFileRule(), source, "/proj/Foo.cs");
            Assert.Empty(diags);
        }

        [Fact]
        public void Logic_SilentForValidCoLocationGroup()
        {
            const string source =
                "using Stricture;\n" +
                "[assembly: OneTypePerFile]\n" +
                "[assembly: CoLocateBySuffix(\"Request\", \"Response\")]\n" +
                "namespace N { public class FooRequest { } public class FooResponse { } }";
            var diags = RuleTestHarness.RunFile(new OneTypePerFileRule(), source, "/proj/Foo.cs");
            Assert.Empty(diags);
        }

        [Fact]
        public async Task Integration_Fail_TwoUnrelatedTypes()
        {
            await StrictureAnalyzerTest.WithSource("/Foo.cs",
                "using Stricture;\n[assembly: OneTypePerFile]\n"
                + "namespace N { public class Foo { } public class {|ARCH2001:Bar|} { } }").RunAsync();
        }

        [Fact]
        public async Task Integration_Pass_SingleType()
        {
            await StrictureAnalyzerTest.WithSource("/Foo.cs",
                "using Stricture;\n[assembly: OneTypePerFile]\nnamespace N { public class Foo { } }").RunAsync();
        }
    }
}
