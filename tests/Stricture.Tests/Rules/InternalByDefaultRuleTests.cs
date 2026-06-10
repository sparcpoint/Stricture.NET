using System.Threading.Tasks;
using Stricture.Rules;
using Stricture.Tests.Infrastructure;
using Xunit;

namespace Stricture.Tests.Rules
{
    /// <summary>ARCH1020 — a public top-level type lacking [PublicApi] when default visibility is internal.</summary>
    public sealed class InternalByDefaultRuleTests
    {
        private const string Policy =
            "using Stricture;\n[assembly: DefaultVisibility(Visibility.Internal)]\n";

        [Fact]
        public void Logic_FiresForPublicTypeWhenInternalByDefault()
        {
            var source = Policy + "namespace N { public class Foo { } }";
            var diags = RuleTestHarness.RunType(new InternalByDefaultRule(), source, "/proj/Foo.cs");
            Assert.Equal("ARCH1020", Assert.Single(diags).Id);
        }

        [Fact]
        public void Logic_SilentForPublicApiMarkedType()
        {
            var source = Policy + "namespace N { [PublicApi] public class Foo { } }";
            var diags = RuleTestHarness.RunType(new InternalByDefaultRule(), source, "/proj/Foo.cs");
            Assert.Empty(diags);
        }

        [Fact]
        public async Task Integration_Fail_PublicTypeWithoutPublicApi()
        {
            await StrictureAnalyzerTest.WithSource("/Foo.cs",
                Policy + "namespace N { public class {|ARCH1020:Foo|} { } }").RunAsync();
        }

        [Fact]
        public async Task Integration_Pass_PublicApiMarked()
        {
            await StrictureAnalyzerTest.WithSource("/Foo.cs",
                Policy + "namespace N { [PublicApi] public class Foo { } }").RunAsync();
        }
    }
}
