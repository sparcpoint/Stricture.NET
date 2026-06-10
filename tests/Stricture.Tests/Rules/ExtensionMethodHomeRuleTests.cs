using System.Threading.Tasks;
using Stricture.Rules;
using Stricture.Tests.Infrastructure;
using Xunit;

namespace Stricture.Tests.Rules
{
    /// <summary>
    /// ARCH1030 — extension methods for a governed type (here <c>IServiceCollection</c>) must be declared
    /// in a partial class named <c>ServiceCollectionExtensions</c> in the configured namespace.
    /// </summary>
    public sealed class ExtensionMethodHomeRuleTests
    {
        private const string Policy =
            @"[assembly: Stricture.ExtensionMethodHome(typeof(Microsoft.Extensions.DependencyInjection.IServiceCollection), ""ServiceCollectionExtensions"", Namespace = ""Microsoft.Extensions.DependencyInjection"")]";

        [Fact]
        public void Logic_SilentForCompliantHost()
        {
            var source = Policy + @"
namespace Microsoft.Extensions.DependencyInjection
{
    public interface IServiceCollection { }
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection AddThing(this IServiceCollection services) => services;
    }
}";
            var diags = RuleTestHarness.RunType(new ExtensionMethodHomeRule(), source, "/proj/ServiceCollectionExtensions.cs");
            Assert.Empty(diags);
        }

        [Fact]
        public void Logic_FiresWhenHostIsMisnamed()
        {
            var source = Policy + @"
namespace Microsoft.Extensions.DependencyInjection
{
    public interface IServiceCollection { }
    public static partial class MyDiExtensions
    {
        public static IServiceCollection AddThing(this IServiceCollection services) => services;
    }
}";
            var diags = RuleTestHarness.RunType(new ExtensionMethodHomeRule(), source, "/proj/MyDiExtensions.cs");
            Assert.Equal("ARCH1030", Assert.Single(diags).Id);
        }

        [Fact]
        public void Logic_FiresWhenHostIsNotPartial()
        {
            var source = Policy + @"
namespace Microsoft.Extensions.DependencyInjection
{
    public interface IServiceCollection { }
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddThing(this IServiceCollection services) => services;
    }
}";
            var diags = RuleTestHarness.RunType(new ExtensionMethodHomeRule(), source, "/proj/ServiceCollectionExtensions.cs");
            Assert.Equal("ARCH1030", Assert.Single(diags).Id);
        }

        [Fact]
        public void Logic_SilentForClassThatHostsNoGovernedExtensions()
        {
            // Same name, not partial — but it declares no IServiceCollection extension, so it is not a host.
            var source = Policy + @"
namespace Microsoft.Extensions.DependencyInjection
{
    public interface IServiceCollection { }
    public static class ServiceCollectionExtensions
    {
        public static void Unrelated() { }
    }
}";
            var diags = RuleTestHarness.RunType(new ExtensionMethodHomeRule(), source, "/proj/ServiceCollectionExtensions.cs");
            Assert.Empty(diags);
        }

        [Fact]
        public async Task Integration_Pass_CompliantHost()
        {
            await StrictureAnalyzerTest.WithSource("/ServiceCollectionExtensions.cs", Policy + @"
namespace Microsoft.Extensions.DependencyInjection
{
    public interface IServiceCollection { }
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection AddThing(this IServiceCollection services) => services;
    }
}").RunAsync();
        }

        [Fact]
        public async Task Integration_Fail_MisnamedHost()
        {
            await StrictureAnalyzerTest.WithSource("/MyDiExtensions.cs", Policy + @"
namespace Microsoft.Extensions.DependencyInjection
{
    public interface IServiceCollection { }
    public static partial class {|ARCH1030:MyDiExtensions|}
    {
        public static IServiceCollection AddThing(this IServiceCollection services) => services;
    }
}").RunAsync();
        }
    }
}
