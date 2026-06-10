using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Stricture.Tests.Infrastructure
{
    /// <summary>An <see cref="AnalyzerOptions"/> shim that exposes <c>build_property.ProjectDir</c> to the engine.</summary>
    internal sealed class TestAnalyzerOptions : AnalyzerOptions
    {
        public TestAnalyzerOptions(string projectDir)
            : base(ImmutableArray<AdditionalText>.Empty, new Provider(projectDir))
        {
        }

        private sealed class Provider : AnalyzerConfigOptionsProvider
        {
            private readonly Options _global;

            public Provider(string projectDir) => _global = new Options(projectDir);

            public override AnalyzerConfigOptions GlobalOptions => _global;

            public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => _global;

            public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => _global;
        }

        private sealed class Options : AnalyzerConfigOptions
        {
            private readonly Dictionary<string, string> _values;

            public Options(string projectDir) =>
                _values = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
                {
                    ["build_property.ProjectDir"] = projectDir,
                };

            public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value) =>
                _values.TryGetValue(key, out value);
        }
    }
}
