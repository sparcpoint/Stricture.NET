using System.Threading.Tasks;
using Stricture.Rules;
using Stricture.Tests.Infrastructure;
using Xunit;

namespace Stricture.Tests.Rules
{
    /// <summary>ARCH1010 — a concrete type named after the interface it implements (X : IX).</summary>
    public sealed class InterfaceNamingRuleTests
    {
        [Fact]
        public void Logic_FiresWhenConcreteTypeNamedAfterItsInterface()
        {
            const string source = "namespace N { public interface IFoo { } public class Foo : IFoo { } }";
            var diags = RuleTestHarness.RunType(new InterfaceNamingRule(), source, "/proj/Foo.cs");
            Assert.Equal("ARCH1010", Assert.Single(diags).Id);
        }

        [Fact]
        public void Logic_SilentForIntentRevealingName()
        {
            const string source = "namespace N { public interface IFoo { } public class Widget : IFoo { } }";
            var diags = RuleTestHarness.RunType(new InterfaceNamingRule(), source, "/proj/Widget.cs");
            Assert.Empty(diags);
        }

        [Fact]
        public async Task Integration_Fail_NamedAfterInterface()
        {
            await StrictureAnalyzerTest.WithSource("/Foo.cs",
                "namespace N { public interface IFoo { } public class {|ARCH1010:Foo|} : IFoo { } }").RunAsync();
        }

        [Fact]
        public async Task Integration_Pass_IntentRevealingName()
        {
            await StrictureAnalyzerTest.WithSource("/Widget.cs",
                "namespace N { public interface IFoo { } public class Widget : IFoo { } }").RunAsync();
        }
    }
}
