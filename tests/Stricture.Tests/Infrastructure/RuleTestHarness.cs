using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Stricture.Tests.Infrastructure
{
    /// <summary>
    /// Fast, MSBuild-free harness for the logic tests. A rule is a pure function of its context, so
    /// each test parses a compilation with an explicit file path and runs a single rule over it.
    /// </summary>
    internal static class RuleTestHarness
    {
        private static readonly ImmutableArray<MetadataReference> References = BuildReferences();

        public static IReadOnlyList<Diagnostic> RunType(TypeRule rule, string source, string path, string projectDir = "/proj/")
        {
            var (shared, compilation) = Setup(source, path, projectDir);
            var diags = new List<Diagnostic>();
            foreach (var type in EnumerateNamedTypes(compilation.Assembly.GlobalNamespace))
            {
                rule.Analyze(new TypeRuleContext(type, shared, diags.Add));
            }

            return diags;
        }

        public static IReadOnlyList<Diagnostic> RunFile(FileRule rule, string source, string path, string projectDir = "/proj/")
        {
            var (shared, compilation) = Setup(source, path, projectDir);
            var tree = compilation.SyntaxTrees.First();
            var diags = new List<Diagnostic>();
            rule.Analyze(new FileRuleContext(tree, shared, diags.Add));
            return diags;
        }

        public static IReadOnlyList<Diagnostic> RunCompilation(CompilationRule rule, string source, string path, string projectDir = "/proj/")
        {
            var (shared, compilation) = Setup(source, path, projectDir);
            var diags = new List<Diagnostic>();
            rule.Analyze(new CompilationRuleContext(compilation, shared, diags.Add));
            return diags;
        }

        public static SharedContext BuildShared(string source, string path, string projectDir = "/proj/")
            => Setup(source, path, projectDir).Shared;

        private static (SharedContext Shared, CSharpCompilation Compilation) Setup(string source, string path, string projectDir)
        {
            var tree = CSharpSyntaxTree.ParseText(SourceText.From(source), path: path);
            var compilation = CSharpCompilation.Create(
                "TestAsm",
                new[] { tree },
                References,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var options = new TestAnalyzerOptions(projectDir);
            var shared = SharedContext.Build(compilation, options);
            return (shared, compilation);
        }

        private static IEnumerable<INamedTypeSymbol> EnumerateNamedTypes(INamespaceSymbol ns)
        {
            foreach (var type in ns.GetTypeMembers())
            {
                foreach (var t in WithNested(type))
                {
                    yield return t;
                }
            }

            foreach (var child in ns.GetNamespaceMembers())
            {
                foreach (var t in EnumerateNamedTypes(child))
                {
                    yield return t;
                }
            }
        }

        private static IEnumerable<INamedTypeSymbol> WithNested(INamedTypeSymbol type)
        {
            yield return type;
            foreach (var nested in type.GetTypeMembers())
            {
                foreach (var t in WithNested(nested))
                {
                    yield return t;
                }
            }
        }

        private static ImmutableArray<MetadataReference> BuildReferences()
        {
            var builder = ImmutableArray.CreateBuilder<MetadataReference>();

            var tpa = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
            if (!string.IsNullOrEmpty(tpa))
            {
                foreach (var file in tpa!.Split(Path.PathSeparator))
                {
                    if (file.Length > 0 && File.Exists(file))
                    {
                        builder.Add(MetadataReference.CreateFromFile(file));
                    }
                }
            }

            // The consumer's attribute library, so [assembly: ...] policy resolves.
            builder.Add(MetadataReference.CreateFromFile(typeof(FolderStructureAttribute).Assembly.Location));
            return builder.ToImmutable();
        }
    }
}
