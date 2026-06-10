using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Stricture.Tests.Guards
{
    /// <summary>
    /// Every assembly-level rule attribute must expose a settable <see cref="Severity"/> property so the
    /// consumer can escalate that rule's violations to errors. Marker attributes that emit no diagnostic
    /// (e.g. [PublicApi]) target types rather than the assembly and are excluded.
    /// </summary>
    public sealed class SeverityPropertyGuardTests
    {
        public static IEnumerable<object[]> AssemblyRuleAttributes() =>
            typeof(FolderStructureAttribute).Assembly
                .GetExportedTypes()
                .Where(t => typeof(Attribute).IsAssignableFrom(t) && TargetsAssembly(t))
                .Select(t => new object[] { t });

        [Theory]
        [MemberData(nameof(AssemblyRuleAttributes))]
        public void AssemblyAttribute_ExposesSettableSeverity(Type attributeType)
        {
            var prop = attributeType.GetProperty("Severity", BindingFlags.Public | BindingFlags.Instance);
            Assert.True(prop != null, $"{attributeType.Name} must expose a public Severity property");
            Assert.Equal(typeof(Severity), prop!.PropertyType);
            Assert.True(prop.CanWrite, $"{attributeType.Name}.Severity must be settable");
        }

        private static bool TargetsAssembly(Type attributeType)
        {
            var usage = attributeType.GetCustomAttribute<AttributeUsageAttribute>();
            return usage != null && (usage.ValidOn & AttributeTargets.Assembly) != 0;
        }
    }
}
