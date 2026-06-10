using Stricture.Rules;
using Stricture.Tests.Infrastructure;
using Xunit;

namespace Stricture.Tests
{
    /// <summary>Fast, MSBuild-free logic tests that run a single rule over a parsed compilation.</summary>
    public sealed class LogicTests
    {
        private const string FolderPolicy =
            "using Stricture;\n" +
            "[assembly: FolderStructure(\"Features\", Pattern = \"{feature}/{category}\", Fallback = \"Models\")]\n" +
            "[assembly: TypeFolder(\"Enumerations\", Kind = TypeShape.Enum)]\n";

        // ---- ARCH1001 ---------------------------------------------------------------------------

        [Fact]
        public void Arch1001_FiresWhenTypeIsInWrongCategoryFolder()
        {
            var source = FolderPolicy + "namespace N { public enum Status { A } }";
            var diags = RuleTestHarness.RunType(new FolderCategoryRule(), source, "/proj/Features/Pay/Models/Status.cs");
            var diag = Assert.Single(diags);
            Assert.Equal("ARCH1001", diag.Id);
        }

        [Fact]
        public void Arch1001_SilentWhenTypeIsInCorrectFolder()
        {
            var source = FolderPolicy + "namespace N { public enum Status { A } }";
            var diags = RuleTestHarness.RunType(new FolderCategoryRule(), source, "/proj/Features/Pay/Enumerations/Status.cs");
            Assert.Empty(diags);
        }

        [Fact]
        public void Arch1001_UsesFallbackWhenTypeIsUnclassified()
        {
            // A plain class classifies to nothing, so it is expected in the structure's Fallback ("Models").
            var source = FolderPolicy + "namespace N { public class Widget { } }";
            var inFallback = RuleTestHarness.RunType(new FolderCategoryRule(), source, "/proj/Features/Pay/Models/Widget.cs");
            Assert.Empty(inFallback);

            var elsewhere = RuleTestHarness.RunType(new FolderCategoryRule(), source, "/proj/Features/Pay/Services/Widget.cs");
            Assert.Equal("ARCH1001", Assert.Single(elsewhere).Id);
        }

        // ---- ARCH1002 ---------------------------------------------------------------------------

        [Fact]
        public void Arch1002_FiresWhenPathDepthDoesNotMatchPattern()
        {
            var source = FolderPolicy + "namespace N { public enum Status { A } }";
            var diags = RuleTestHarness.RunType(new StructurePatternRule(), source, "/proj/Features/Status.cs");
            Assert.Equal("ARCH1002", Assert.Single(diags).Id);
        }

        [Fact]
        public void Arch1002_SilentWhenPathMatchesPattern()
        {
            var source = FolderPolicy + "namespace N { public enum Status { A } }";
            var diags = RuleTestHarness.RunType(new StructurePatternRule(), source, "/proj/Features/Pay/Enumerations/Status.cs");
            Assert.Empty(diags);
        }

        // ---- ARCH1003 ---------------------------------------------------------------------------

        [Fact]
        public void Arch1003_FiresForPublicNestedClassifyingType()
        {
            var source = FolderPolicy + "namespace N { public class Outer { public enum Inner { A } } }";
            var diags = RuleTestHarness.RunType(new NestedPromotionRule(), source, "/proj/Features/Pay/Enumerations/Outer.cs");
            var diag = Assert.Single(diags);
            Assert.Equal("ARCH1003", diag.Id);
        }

        [Fact]
        public void Arch1003_SilentForPrivateNestedType()
        {
            var source = FolderPolicy + "namespace N { public class Outer { private enum Inner { A } } }";
            var diags = RuleTestHarness.RunType(new NestedPromotionRule(), source, "/proj/Features/Pay/Enumerations/Outer.cs");
            Assert.Empty(diags);
        }

        // ---- ARCH1010 ---------------------------------------------------------------------------

        [Fact]
        public void Arch1010_FiresWhenConcreteTypeNamedAfterItsInterface()
        {
            const string source = "namespace N { public interface IFoo { } public class Foo : IFoo { } }";
            var diags = RuleTestHarness.RunType(new InterfaceNamingRule(), source, "/proj/Foo.cs");
            Assert.Equal("ARCH1010", Assert.Single(diags).Id);
        }

        [Fact]
        public void Arch1010_SilentForIntentRevealingName()
        {
            const string source = "namespace N { public interface IFoo { } public class Widget : IFoo { } }";
            var diags = RuleTestHarness.RunType(new InterfaceNamingRule(), source, "/proj/Widget.cs");
            Assert.Empty(diags);
        }

        // ---- ARCH1020 ---------------------------------------------------------------------------

        [Fact]
        public void Arch1020_FiresForPublicTypeWhenInternalByDefault()
        {
            const string source =
                "using Stricture;\n" +
                "[assembly: DefaultVisibility(Visibility.Internal)]\n" +
                "namespace N { public class Foo { } }";
            var diags = RuleTestHarness.RunType(new InternalByDefaultRule(), source, "/proj/Foo.cs");
            Assert.Equal("ARCH1020", Assert.Single(diags).Id);
        }

        [Fact]
        public void Arch1020_SilentForPublicApiMarkedType()
        {
            const string source =
                "using Stricture;\n" +
                "[assembly: DefaultVisibility(Visibility.Internal)]\n" +
                "namespace N { [PublicApi] public class Foo { } }";
            var diags = RuleTestHarness.RunType(new InternalByDefaultRule(), source, "/proj/Foo.cs");
            Assert.Empty(diags);
        }

        // ---- ARCH2001 ---------------------------------------------------------------------------

        [Fact]
        public void Arch2001_FiresForMultipleUnrelatedTopLevelTypes()
        {
            const string source =
                "using Stricture;\n" +
                "[assembly: OneTypePerFile]\n" +
                "namespace N { public class Foo { } public class Bar { } }";
            var diags = RuleTestHarness.RunFile(new OneTypePerFileRule(), source, "/proj/Foo.cs");
            var diag = Assert.Single(diags);
            Assert.Equal("ARCH2001", diag.Id);
        }

        [Fact]
        public void Arch2001_SilentForSingleTopLevelType()
        {
            const string source =
                "using Stricture;\n" +
                "[assembly: OneTypePerFile]\n" +
                "namespace N { public class Foo { } }";
            var diags = RuleTestHarness.RunFile(new OneTypePerFileRule(), source, "/proj/Foo.cs");
            Assert.Empty(diags);
        }

        [Fact]
        public void Arch2001_SilentForValidCoLocationGroup()
        {
            const string source =
                "using Stricture;\n" +
                "[assembly: OneTypePerFile]\n" +
                "[assembly: CoLocateBySuffix(\"Request\", \"Response\")]\n" +
                "namespace N { public class FooRequest { } public class FooResponse { } }";
            var diags = RuleTestHarness.RunFile(new OneTypePerFileRule(), source, "/proj/Foo.cs");
            Assert.Empty(diags);
        }

        // ---- ARCH2002 ---------------------------------------------------------------------------

        [Fact]
        public void Arch2002_FiresForMismatchedStems()
        {
            const string source =
                "using Stricture;\n" +
                "[assembly: OneTypePerFile]\n" +
                "[assembly: CoLocateBySuffix(\"Request\", \"Response\")]\n" +
                "namespace N { public class FooRequest { } public class BarResponse { } }";
            var diags = RuleTestHarness.RunFile(new CoLocationRule(), source, "/proj/Foo.cs");
            Assert.Equal("ARCH2002", Assert.Single(diags).Id);
        }

        [Fact]
        public void Arch2002_SilentForSharedStem()
        {
            const string source =
                "using Stricture;\n" +
                "[assembly: OneTypePerFile]\n" +
                "[assembly: CoLocateBySuffix(\"Request\", \"Response\")]\n" +
                "namespace N { public class FooRequest { } public class FooResponse { } }";
            var diags = RuleTestHarness.RunFile(new CoLocationRule(), source, "/proj/Foo.cs");
            Assert.Empty(diags);
        }

        // ---- ARCH0001 ---------------------------------------------------------------------------

        [Fact]
        public void Arch0001_FiresForPatternMissingCategory()
        {
            const string source =
                "using Stricture;\n" +
                "[assembly: FolderStructure(\"Features\", Pattern = \"{feature}\")]\n";
            var diags = RuleTestHarness.RunCompilation(new ConfigValidationRule(), source, "/proj/Architecture.cs");
            Assert.Equal("ARCH0001", Assert.Single(diags).Id);
        }

        [Fact]
        public void Arch0001_SilentForValidConfiguration()
        {
            const string source =
                "using Stricture;\n" +
                "[assembly: FolderStructure(\"Features\", Pattern = \"{feature}/{category}\")]\n";
            var diags = RuleTestHarness.RunCompilation(new ConfigValidationRule(), source, "/proj/Architecture.cs");
            Assert.Empty(diags);
        }

        // ---- ARCH4001 ---------------------------------------------------------------------------

        [Fact]
        public void Arch4001_FiresForBannedReferencedAssembly()
        {
            const string source =
                "using Stricture;\n" +
                "[assembly: BanPackage(\"Stricture.Abstractions\")]\n";
            var diags = RuleTestHarness.RunCompilation(new BannedReferenceRule(), source, "/proj/Architecture.cs");
            Assert.Equal("ARCH4001", Assert.Single(diags).Id);
        }

        [Fact]
        public void Arch4001_SilentWhenBannedAssemblyNotReferenced()
        {
            const string source =
                "using Stricture;\n" +
                "[assembly: BanPackage(\"NoSuchAssembly\")]\n";
            var diags = RuleTestHarness.RunCompilation(new BannedReferenceRule(), source, "/proj/Architecture.cs");
            Assert.Empty(diags);
        }
    }
}
