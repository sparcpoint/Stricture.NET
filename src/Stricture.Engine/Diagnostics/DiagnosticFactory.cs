using System.Globalization;
using Microsoft.CodeAnalysis;

namespace Stricture
{
    /// <summary>
    /// Creates diagnostics honoring a per-instance severity. When the requested severity equals the
    /// descriptor's default, the standard descriptor-based factory is used (preserving the localizable
    /// message). Otherwise the explicit overload re-emits the diagnostic under the chosen severity while
    /// keeping the same id, so <c>.editorconfig</c>/ruleset overrides continue to take precedence.
    /// </summary>
    internal static class DiagnosticFactory
    {
        public static Diagnostic Create(
            DiagnosticDescriptor descriptor, DiagnosticSeverity severity, Location location, object[] messageArgs)
        {
            if (severity == descriptor.DefaultSeverity)
            {
                return Diagnostic.Create(descriptor, location, messageArgs);
            }

            var message = string.Format(
                CultureInfo.InvariantCulture,
                descriptor.MessageFormat.ToString(CultureInfo.InvariantCulture),
                messageArgs);

            return Diagnostic.Create(
                descriptor.Id,
                descriptor.Category,
                message,
                severity,
                descriptor.DefaultSeverity,
                descriptor.IsEnabledByDefault,
                warningLevel: severity == DiagnosticSeverity.Error ? 0 : 1,
                title: descriptor.Title.ToString(CultureInfo.InvariantCulture),
                description: descriptor.Description.ToString(CultureInfo.InvariantCulture),
                helpLink: descriptor.HelpLinkUri,
                location: location,
                customTags: descriptor.CustomTags);
        }
    }
}
