using Microsoft.CodeAnalysis;

namespace Stricture
{
    /// <summary>All diagnostic descriptors the engine can report.</summary>
    internal static class Descriptors
    {
        private const string ConfigCategory = "Stricture.Config";
        private const string EngineCategory = "Stricture.Engine";
        private const string LayoutCategory = "Stricture.Layout";
        private const string NamingCategory = "Stricture.Naming";
        private const string VisibilityCategory = "Stricture.Visibility";
        private const string BansCategory = "Stricture.Bans";

        private const string HelpUri = "https://github.com/sparcpoint/Stricture.NET/blob/main/README.md#diagnostics";

        public static readonly DiagnosticDescriptor Arch0001 = Make(
            "ARCH0001", ConfigCategory, "Invalid Stricture configuration",
            "Invalid Stricture configuration: {0}");

        public static readonly DiagnosticDescriptor Arch0002 = Make(
            "ARCH0002", EngineCategory, "Stricture rule error",
            "Stricture rule '{0}' failed: {1}");

        public static readonly DiagnosticDescriptor Arch1001 = Make(
            "ARCH1001", LayoutCategory, "Type in wrong category folder",
            "Type '{0}' should be in folder '{1}' but is in '{2}'");

        public static readonly DiagnosticDescriptor Arch1002 = Make(
            "ARCH1002", LayoutCategory, "Type path does not match structure pattern",
            "Type '{0}' is under a structure root but its path does not match pattern '{1}'");

        public static readonly DiagnosticDescriptor Arch1003 = Make(
            "ARCH1003", LayoutCategory, "Nested type should be promoted to its own file",
            "'{0}' is a nested {1}; promote it to its own file in '{2}'");

        public static readonly DiagnosticDescriptor Arch1010 = Make(
            "ARCH1010", NamingCategory, "Concrete type named after its interface",
            "Type '{0}' must not be named after interface '{1}'; choose an intent-revealing name");

        public static readonly DiagnosticDescriptor Arch1020 = Make(
            "ARCH1020", VisibilityCategory, "Public type should be internal by default",
            "Type '{0}' should be internal by default; mark it [PublicApi] if the surface is intentional");

        public static readonly DiagnosticDescriptor Arch2001 = Make(
            "ARCH2001", LayoutCategory, "More than one top-level type per file",
            "File declares multiple top-level types; '{0}' should move to its own file");

        public static readonly DiagnosticDescriptor Arch2002 = Make(
            "ARCH2002", LayoutCategory, "Co-located types violate suffix or stem rules",
            "Co-located types in this file violate the co-location policy: {0}");

        public static readonly DiagnosticDescriptor Arch3001 = Make(
            "ARCH3001", BansCategory, "Usage of a banned type or namespace",
            "Banned API usage: {0}");

        public static readonly DiagnosticDescriptor Arch4001 = Make(
            "ARCH4001", BansCategory, "Reference to a banned assembly",
            "Banned assembly reference: {0}");

        private static DiagnosticDescriptor Make(string id, string category, string title, string messageFormat) =>
            new DiagnosticDescriptor(
                id,
                title,
                messageFormat,
                category,
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                description: null,
                helpLinkUri: HelpUri);
    }
}
