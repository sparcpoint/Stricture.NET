using System.Threading.Tasks;
using Stricture.Tests.Infrastructure;
using Xunit;

namespace Stricture.Tests.Rules
{
    /// <summary>
    /// ARCH3001 — usage of a banned type or namespace. Covered by integration only: the rule operates on
    /// <c>IOperation</c>s, which require the full analyzer pipeline rather than the fast logic harness.
    /// </summary>
    public sealed class BannedUsageRuleTests
    {
        private const string Policy =
            "using Stricture;\n[assembly: BanType(FullyQualifiedName = \"System.Text.StringBuilder\")]\n";

        [Fact]
        public async Task Integration_Fail_BannedTypeUsage()
        {
            await StrictureAnalyzerTest.WithSource("/C.cs",
                Policy + "namespace N { internal class C { System.Text.StringBuilder Make() => {|ARCH3001:new System.Text.StringBuilder()|}; } }")
                .RunAsync();
        }

        [Fact]
        public async Task Integration_Pass_NoBannedUsage()
        {
            await StrictureAnalyzerTest.WithSource("/C.cs",
                Policy + "namespace N { internal class C { int Make() => 42; } }").RunAsync();
        }
    }
}
