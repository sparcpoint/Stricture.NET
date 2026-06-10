using System.Threading.Tasks;
using Stricture.Tests.Infrastructure;
using Xunit;

namespace Stricture.Tests
{
    /// <summary>
    /// The no-false-positive guard: a realistic, fully-compliant policy with correctly-placed types
    /// must yield zero diagnostics through the whole analyzer (mirrors <c>samples/CleanSample</c>).
    /// </summary>
    public sealed class NoFalsePositiveTests
    {
        private const string Architecture =
            "using System;\n" +
            "using Stricture;\n" +
            "[assembly: FolderStructure(\"Features\", Pattern = \"{feature}/{category}\", Fallback = \"Models\")]\n" +
            "[assembly: FolderStructure(\"Platform\", Pattern = \"{area}/{category}\", Fallback = \"Internal\")]\n" +
            "[assembly: TypeFolder(\"Enumerations\", Kind = TypeShape.Enum)]\n" +
            "[assembly: TypeFolder(\"Exceptions\", DerivesFrom = typeof(Exception))]\n" +
            "[assembly: TypeFolder(\"Requests\", EndsWith = \"Request\")]\n" +
            "[assembly: TypeFolder(\"Requests\", EndsWith = \"Response\")]\n" +
            "[assembly: TypeFolder(\"Models\", IsRecord = true)]\n" +
            "[assembly: OneTypePerFile]\n" +
            "[assembly: CoLocateBySuffix(\"Request\", \"Response\")]\n" +
            "[assembly: DefaultVisibility(Visibility.Internal)]\n" +
            "[assembly: BanPackage(\"Moq\")]\n";

        [Fact]
        public async Task FullyCompliantPolicy_ProducesNoDiagnostics()
        {
            var test = new StrictureAnalyzerTest();
            test.TestState.Sources.Add(("/Architecture.cs", Architecture));
            test.TestState.Sources.Add(("/Features/Pay/Models/Payment.cs",
                "namespace N.Models { internal sealed record Payment(string Id, decimal Amount); }"));
            test.TestState.Sources.Add(("/Features/Pay/Enumerations/PaymentStatus.cs",
                "namespace N.Enums { internal enum PaymentStatus { Pending, Settled } }"));
            test.TestState.Sources.Add(("/Features/Pay/Exceptions/PaymentFailedException.cs",
                "using System;\nnamespace N.Ex { internal sealed class PaymentFailedException : Exception { } }"));
            test.TestState.Sources.Add(("/Features/Orders/Requests/CreateOrder.cs",
                "namespace N.Req { internal sealed class CreateOrderRequest { } internal sealed class CreateOrderResponse { } }"));
            test.TestState.Sources.Add(("/Platform/Logging/Internal/LogScope.cs",
                "namespace N.Log { internal sealed class LogScope { } }"));

            await test.RunAsync();
        }
    }
}
