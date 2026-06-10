using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Stricture
{
    /// <summary>
    /// The single root analyzer. It discovers all rules in this assembly by reflection, builds the
    /// shared context once per compilation, and dispatches to each rule with per-rule isolation.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class StrictureAnalyzer : DiagnosticAnalyzer
    {
        private static readonly ImmutableArray<TypeRule> TypeRules = Discover<TypeRule>();
        private static readonly ImmutableArray<FileRule> FileRules = Discover<FileRule>();
        private static readonly ImmutableArray<OperationRule> OperationRules = Discover<OperationRule>();
        private static readonly ImmutableArray<CompilationRule> CompilationRules = Discover<CompilationRule>();

        private static readonly ImmutableArray<DiagnosticDescriptor> AllDescriptors = BuildDescriptors();

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => AllDescriptors;

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(start =>
            {
                var shared = SharedContext.Build(start.Compilation, start.Options);

                start.RegisterSymbolAction(
                    sc =>
                    {
                        if (sc.Symbol is not INamedTypeSymbol named)
                        {
                            return;
                        }

                        var ctx = new TypeRuleContext(named, shared, sc.ReportDiagnostic);
                        foreach (var rule in TypeRules)
                        {
                            RunIsolated(rule, () => rule.Analyze(ctx), sc.ReportDiagnostic, named.Locations.FirstOrDefault());
                        }
                    },
                    SymbolKind.NamedType);

                start.RegisterSyntaxTreeAction(tc =>
                {
                    if (IsGenerated(tc.Tree.FilePath))
                    {
                        return;
                    }

                    var ctx = new FileRuleContext(tc.Tree, shared, tc.ReportDiagnostic);
                    foreach (var rule in FileRules)
                    {
                        RunIsolated(rule, () => rule.Analyze(ctx), tc.ReportDiagnostic, null);
                    }
                });

                start.RegisterOperationAction(
                    oc =>
                    {
                        var ctx = new OperationRuleContext(oc.Operation, shared, oc.ReportDiagnostic);
                        foreach (var rule in OperationRules)
                        {
                            RunIsolated(rule, () => rule.Analyze(ctx), oc.ReportDiagnostic, oc.Operation.Syntax.GetLocation());
                        }
                    },
                    OperationKind.Invocation,
                    OperationKind.ObjectCreation,
                    OperationKind.FieldReference,
                    OperationKind.PropertyReference,
                    OperationKind.MethodReference);

                start.RegisterCompilationEndAction(end =>
                {
                    var ctx = new CompilationRuleContext(end.Compilation, shared, end.ReportDiagnostic);
                    foreach (var rule in CompilationRules)
                    {
                        RunIsolated(rule, () => rule.Analyze(ctx), end.ReportDiagnostic, null);
                    }
                });
            });
        }

        internal static void RunIsolated(object rule, Action analyze, Action<Diagnostic> report, Location? location)
        {
            try
            {
                analyze();
            }
#pragma warning disable CA1031 // Per-rule isolation is intentional: one buggy rule must not kill analysis.
            catch (Exception ex)
#pragma warning restore CA1031
            {
                report(Diagnostic.Create(Descriptors.Arch0002, location ?? Location.None, rule.GetType().Name, ex.Message));
            }
        }

        private static bool IsGenerated(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            var name = PathUtil.GetFileName(filePath);
            return name.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase)
                || name.EndsWith(".g.i.cs", StringComparison.OrdinalIgnoreCase)
                || name.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase);
        }

        private static ImmutableArray<DiagnosticDescriptor> BuildDescriptors()
        {
            var set = new Dictionary<string, DiagnosticDescriptor>(StringComparer.Ordinal);

            void Add(DiagnosticDescriptor d)
            {
                if (!set.ContainsKey(d.Id))
                {
                    set.Add(d.Id, d);
                }
            }

            Add(Descriptors.Arch0002);
            foreach (var r in TypeRules)
            {
                Add(r.Descriptor);
            }

            foreach (var r in FileRules)
            {
                Add(r.Descriptor);
            }

            foreach (var r in OperationRules)
            {
                Add(r.Descriptor);
            }

            foreach (var r in CompilationRules)
            {
                foreach (var d in r.Descriptors)
                {
                    Add(d);
                }
            }

            return set.Values.ToImmutableArray();
        }

        private static ImmutableArray<T> Discover<T>()
            where T : class
        {
            var builder = ImmutableArray.CreateBuilder<T>();
            foreach (var type in typeof(StrictureAnalyzer).Assembly.GetTypes())
            {
                if (type.IsClass && !type.IsAbstract && typeof(T).IsAssignableFrom(type)
                    && type.GetConstructor(Type.EmptyTypes) != null)
                {
                    builder.Add((T)Activator.CreateInstance(type)!);
                }
            }

            return builder.ToImmutable();
        }
    }
}
