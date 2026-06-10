using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Stricture.Tests.Infrastructure;
using Xunit;

namespace Stricture.Tests
{
    /// <summary>
    /// Wiring/severity/config-reading tests that run the whole <see cref="StrictureAnalyzer"/> and assert
    /// at least one passing and one failing fixture per diagnostic id. Failing fixtures use markup spans;
    /// no-location diagnostics use explicit <see cref="DiagnosticResult"/>s.
    /// </summary>
    public sealed class IntegrationTests
    {
        private static StrictureAnalyzerTest Test(string path, string source)
        {
            var test = new StrictureAnalyzerTest();
            test.TestState.Sources.Add((path, source));
            return test;
        }

        // ---- ARCH0001: invalid config -----------------------------------------------------------

        [Fact]
        public async Task Arch0001_Fail_PatternMissingCategory()
        {
            var test = Test("/Policy.cs",
                "using Stricture;\n[assembly: FolderStructure(\"Features\", Pattern = \"{feature}\")]\n");
            test.ExpectedDiagnostics.Add(new DiagnosticResult("ARCH0001", DiagnosticSeverity.Warning).WithNoLocation());
            await test.RunAsync();
        }

        [Fact]
        public async Task Arch0001_Pass_ValidConfig()
        {
            await Test("/Policy.cs",
                "using Stricture;\n[assembly: FolderStructure(\"Features\", Pattern = \"{feature}/{category}\")]\n")
                .RunAsync();
        }

        // ---- ARCH1001: wrong category folder ----------------------------------------------------

        private const string FolderPolicy =
            "using Stricture;\n" +
            "[assembly: FolderStructure(\"Features\", Pattern = \"{feature}/{category}\", Fallback = \"Models\")]\n" +
            "[assembly: TypeFolder(\"Enumerations\", Kind = TypeShape.Enum)]\n";

        [Fact]
        public async Task Arch1001_Fail_WrongFolder()
        {
            await Test("/Features/Pay/Models/Status.cs",
                FolderPolicy + "namespace N { public enum {|ARCH1001:Status|} { A } }").RunAsync();
        }

        [Fact]
        public async Task Arch1001_Pass_CorrectFolder()
        {
            await Test("/Features/Pay/Enumerations/Status.cs",
                FolderPolicy + "namespace N { public enum Status { A } }").RunAsync();
        }

        // ---- ARCH1002: path does not match pattern ----------------------------------------------

        [Fact]
        public async Task Arch1002_Fail_WrongDepth()
        {
            await Test("/Features/Status.cs",
                "using Stricture;\n[assembly: FolderStructure(\"Features\", Pattern = \"{feature}/{category}\")]\n"
                + "namespace N { public enum {|ARCH1002:Status|} { A } }").RunAsync();
        }

        [Fact]
        public async Task Arch1002_Pass_CorrectDepth()
        {
            await Test("/Features/Pay/Misc/Status.cs",
                "using Stricture;\n[assembly: FolderStructure(\"Features\", Pattern = \"{feature}/{category}\")]\n"
                + "namespace N { public enum Status { A } }").RunAsync();
        }

        // ---- ARCH1003: nested type should be promoted -------------------------------------------

        [Fact]
        public async Task Arch1003_Fail_PublicNestedClassifyingType()
        {
            await Test("/Other/Outer.cs",
                FolderPolicy + "namespace N { public class Outer { public enum {|ARCH1003:Inner|} { A } } }").RunAsync();
        }

        [Fact]
        public async Task Arch1003_Pass_PrivateNestedType()
        {
            await Test("/Other/Outer.cs",
                FolderPolicy + "namespace N { public class Outer { private enum Inner { A } } }").RunAsync();
        }

        // ---- ARCH1010: concrete type named after its interface ----------------------------------

        [Fact]
        public async Task Arch1010_Fail_NamedAfterInterface()
        {
            await Test("/Foo.cs",
                "namespace N { public interface IFoo { } public class {|ARCH1010:Foo|} : IFoo { } }").RunAsync();
        }

        [Fact]
        public async Task Arch1010_Pass_IntentRevealingName()
        {
            await Test("/Widget.cs",
                "namespace N { public interface IFoo { } public class Widget : IFoo { } }").RunAsync();
        }

        // ---- ARCH1020: internal by default ------------------------------------------------------

        [Fact]
        public async Task Arch1020_Fail_PublicTypeWithoutPublicApi()
        {
            await Test("/Foo.cs",
                "using Stricture;\n[assembly: DefaultVisibility(Visibility.Internal)]\n"
                + "namespace N { public class {|ARCH1020:Foo|} { } }").RunAsync();
        }

        [Fact]
        public async Task Arch1020_Pass_PublicApiMarked()
        {
            await Test("/Foo.cs",
                "using Stricture;\n[assembly: DefaultVisibility(Visibility.Internal)]\n"
                + "namespace N { [PublicApi] public class Foo { } }").RunAsync();
        }

        // ---- ARCH2001: more than one top-level type per file ------------------------------------

        [Fact]
        public async Task Arch2001_Fail_TwoUnrelatedTypes()
        {
            await Test("/Foo.cs",
                "using Stricture;\n[assembly: OneTypePerFile]\n"
                + "namespace N { public class Foo { } public class {|ARCH2001:Bar|} { } }").RunAsync();
        }

        [Fact]
        public async Task Arch2001_Pass_SingleType()
        {
            await Test("/Foo.cs",
                "using Stricture;\n[assembly: OneTypePerFile]\nnamespace N { public class Foo { } }").RunAsync();
        }

        // ---- ARCH2002: co-location suffix/stem violations ---------------------------------------

        [Fact]
        public async Task Arch2002_Fail_MismatchedStems()
        {
            await Test("/Foo.cs",
                "using Stricture;\n[assembly: OneTypePerFile]\n[assembly: CoLocateBySuffix(\"Request\", \"Response\")]\n"
                + "namespace N { public class {|ARCH2002:FooRequest|} { } public class BarResponse { } }").RunAsync();
        }

        [Fact]
        public async Task Arch2002_Pass_SharedStem()
        {
            await Test("/Foo.cs",
                "using Stricture;\n[assembly: OneTypePerFile]\n[assembly: CoLocateBySuffix(\"Request\", \"Response\")]\n"
                + "namespace N { public class FooRequest { } public class FooResponse { } }").RunAsync();
        }

        // ---- ARCH3001: banned type/namespace usage ----------------------------------------------

        [Fact]
        public async Task Arch3001_Fail_BannedTypeUsage()
        {
            await Test("/C.cs",
                "using Stricture;\n[assembly: BanType(FullyQualifiedName = \"System.Text.StringBuilder\")]\n"
                + "namespace N { internal class C { System.Text.StringBuilder Make() => {|ARCH3001:new System.Text.StringBuilder()|}; } }")
                .RunAsync();
        }

        [Fact]
        public async Task Arch3001_Pass_NoBannedUsage()
        {
            await Test("/C.cs",
                "using Stricture;\n[assembly: BanType(FullyQualifiedName = \"System.Text.StringBuilder\")]\n"
                + "namespace N { internal class C { int Make() => 42; } }").RunAsync();
        }

        // ---- ARCH4001: banned assembly reference ------------------------------------------------

        [Fact]
        public async Task Arch4001_Fail_BannedReference()
        {
            var test = Test("/Policy.cs",
                "using Stricture;\n[assembly: BanPackage(\"Stricture.Abstractions\")]\n");
            test.ExpectedDiagnostics.Add(new DiagnosticResult("ARCH4001", DiagnosticSeverity.Warning).WithNoLocation());
            await test.RunAsync();
        }

        [Fact]
        public async Task Arch4001_Pass_NoBannedReference()
        {
            await Test("/Policy.cs",
                "using Stricture;\n[assembly: BanPackage(\"NoSuchAssembly\")]\n").RunAsync();
        }
    }
}
