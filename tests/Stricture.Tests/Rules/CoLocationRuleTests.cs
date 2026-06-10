using System.Threading.Tasks;
using Stricture.Rules;
using Stricture.Tests.Infrastructure;
using Xunit;

namespace Stricture.Tests.Rules
{
    /// <summary>ARCH2002 — co-located types mix suffix groups or (when required) have mismatched stems.</summary>
    public sealed class CoLocationRuleTests
    {
        private const string Policy =
            "using Stricture;\n[assembly: OneTypePerFile]\n[assembly: CoLocateBySuffix(\"Request\", \"Response\")]\n";

        [Fact]
        public void Logic_FiresForMismatchedStems()
        {
            var source = Policy + "namespace N { public class FooRequest { } public class BarResponse { } }";
            var diags = RuleTestHarness.RunFile(new CoLocationRule(), source, "/proj/Foo.cs");
            Assert.Equal("ARCH2002", Assert.Single(diags).Id);
        }

        [Fact]
        public void Logic_SilentForSharedStem()
        {
            var source = Policy + "namespace N { public class FooRequest { } public class FooResponse { } }";
            var diags = RuleTestHarness.RunFile(new CoLocationRule(), source, "/proj/Foo.cs");
            Assert.Empty(diags);
        }

        [Fact]
        public async Task Integration_Fail_MismatchedStems()
        {
            await StrictureAnalyzerTest.WithSource("/Foo.cs",
                Policy + "namespace N { public class {|ARCH2002:FooRequest|} { } public class BarResponse { } }").RunAsync();
        }

        [Fact]
        public async Task Integration_Pass_SharedStem()
        {
            await StrictureAnalyzerTest.WithSource("/Foo.cs",
                Policy + "namespace N { public class FooRequest { } public class FooResponse { } }").RunAsync();
        }
    }
}
