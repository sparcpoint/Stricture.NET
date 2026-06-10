using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Stricture.Rules;
using Stricture.Tests.Infrastructure;
using Xunit;

namespace Stricture.Tests.Rules
{
    /// <summary>ARCH1010 — a concrete type named after the interface it implements (X : IX).</summary>
    public sealed class InterfaceNamingRuleTests
    {
        // The rule is opt-in: it only fires when [ForbidInterfaceNaming] is declared.
        private const string Policy = "[assembly: Stricture.ForbidInterfaceNaming]\n";

        [Fact]
        public void Logic_FiresWhenConcreteTypeNamedAfterItsInterface()
        {
            const string source = Policy + "namespace N { public interface IFoo { } public class Foo : IFoo { } }";
            var diags = RuleTestHarness.RunType(new InterfaceNamingRule(), source, "/proj/Foo.cs");
            var diag = Assert.Single(diags);
            Assert.Equal("ARCH1010", diag.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diag.Severity);
        }

        [Fact]
        public void Logic_DisabledWhenAttributeAbsent()
        {
            // No [ForbidInterfaceNaming]: the opt-in rule must stay silent.
            const string source = "namespace N { public interface IFoo { } public class Foo : IFoo { } }";
            var diags = RuleTestHarness.RunType(new InterfaceNamingRule(), source, "/proj/Foo.cs");
            Assert.Empty(diags);
        }

        [Fact]
        public void Logic_ReportsAsErrorWhenSeverityIsError()
        {
            const string source =
                "[assembly: Stricture.ForbidInterfaceNaming(Severity = Stricture.Severity.Error)]\n"
                + "namespace N { public interface IFoo { } public class Foo : IFoo { } }";
            var diags = RuleTestHarness.RunType(new InterfaceNamingRule(), source, "/proj/Foo.cs");
            var diag = Assert.Single(diags);
            Assert.Equal("ARCH1010", diag.Id);
            Assert.Equal(DiagnosticSeverity.Error, diag.Severity);
        }

        [Fact]
        public void Logic_SilentForIntentRevealingName()
        {
            const string source = Policy + "namespace N { public interface IFoo { } public class Widget : IFoo { } }";
            var diags = RuleTestHarness.RunType(new InterfaceNamingRule(), source, "/proj/Widget.cs");
            Assert.Empty(diags);
        }

        [Fact]
        public async Task Integration_Fail_NamedAfterInterface()
        {
            await StrictureAnalyzerTest.WithSource("/Foo.cs",
                Policy + "namespace N { public interface IFoo { } public class {|ARCH1010:Foo|} : IFoo { } }").RunAsync();
        }

        [Fact]
        public async Task Integration_Pass_IntentRevealingName()
        {
            await StrictureAnalyzerTest.WithSource("/Widget.cs",
                Policy + "namespace N { public interface IFoo { } public class Widget : IFoo { } }").RunAsync();
        }
    }
}
