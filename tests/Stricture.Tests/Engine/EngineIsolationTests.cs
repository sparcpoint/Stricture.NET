using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Stricture.Tests.Engine
{
    /// <summary>ARCH0002: a rule that throws is isolated and surfaced as an engine-error diagnostic.</summary>
    public sealed class EngineIsolationTests
    {
        [Fact]
        public void RunIsolated_ReportsArch0002_WhenRuleThrows()
        {
            var diags = new List<Diagnostic>();
            StrictureAnalyzer.RunIsolated(
                this,
                () => throw new InvalidOperationException("boom"),
                diags.Add,
                Location.None);

            var diag = Assert.Single(diags);
            Assert.Equal("ARCH0002", diag.Id);
            Assert.Contains("boom", diag.GetMessage(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
        }

        [Fact]
        public void RunIsolated_ReportsNothing_WhenRuleSucceeds()
        {
            var diags = new List<Diagnostic>();
            StrictureAnalyzer.RunIsolated(this, () => { }, diags.Add, Location.None);
            Assert.Empty(diags);
        }
    }
}
