using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Stricture
{
    /// <summary>The per-type context: a cheap wrapper over <see cref="SharedContext"/> plus type sugar.</summary>
    public sealed class TypeRuleContext
    {
        private readonly Action<Diagnostic> _report;

        /// <summary>Creates a context for one type.</summary>
        public TypeRuleContext(INamedTypeSymbol type, SharedContext shared, Action<Diagnostic> report)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Shared = shared ?? throw new ArgumentNullException(nameof(shared));
            _report = report ?? throw new ArgumentNullException(nameof(report));
        }

        /// <summary>The type under analysis.</summary>
        public INamedTypeSymbol Type { get; }

        /// <summary>The shared compilation context.</summary>
        public SharedContext Shared { get; }

        /// <summary>The type's simple name.</summary>
        public string TypeName => Type.Name;

        /// <summary>True when the type has no containing type.</summary>
        public bool IsTopLevel => Type.ContainingType is null;

        /// <summary>True when the type is nested.</summary>
        public bool IsNested => !IsTopLevel;

        /// <summary>The type's declared accessibility.</summary>
        public Accessibility Accessibility => Type.DeclaredAccessibility;

        /// <summary>True for record types.</summary>
        public bool IsRecord => Type.IsRecord;

        /// <summary>True for enum types.</summary>
        public bool IsEnum => Type.TypeKind == TypeKind.Enum;

        /// <summary>The path of the first in-source location's syntax tree.</summary>
        public string? FilePath =>
            Type.Locations.FirstOrDefault(l => l.IsInSource)?.SourceTree?.FilePath;

        /// <summary>The leaf directory name of <see cref="FilePath"/>.</summary>
        public string? Folder => PathUtil.GetLeafFolder(FilePath);

        /// <summary>A human word for the type's kind (e.g. <c>class</c>, <c>record</c>).</summary>
        public string KindWord
        {
            get
            {
                if (Type.IsRecord)
                {
                    return Type.TypeKind == TypeKind.Struct ? "record struct" : "record";
                }

                switch (Type.TypeKind)
                {
                    case TypeKind.Class: return "class";
                    case TypeKind.Struct: return "struct";
                    case TypeKind.Interface: return "interface";
                    case TypeKind.Enum: return "enum";
                    case TypeKind.Delegate: return "delegate";
                    default: return "type";
                }
            }
        }

        /// <summary>True when the type derives from <paramref name="baseType"/>.</summary>
        public bool DerivesFrom(INamedTypeSymbol baseType)
        {
            var current = Type.BaseType;
            while (current != null)
            {
                if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, baseType.OriginalDefinition))
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        }

        /// <summary>True when the type implements <paramref name="iface"/>.</summary>
        public bool Implements(INamedTypeSymbol iface) =>
            Type.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, iface.OriginalDefinition));

        /// <summary>True when the type carries the attribute <paramref name="attr"/>.</summary>
        public bool HasAttribute(INamedTypeSymbol attr) =>
            Type.GetAttributes().Any(a => a.AttributeClass != null
                && SymbolEqualityComparer.Default.Equals(a.AttributeClass.OriginalDefinition, attr.OriginalDefinition));

        /// <summary>True when the type carries an attribute whose metadata name equals <paramref name="metadataName"/>.</summary>
        public bool HasAttributeNamed(string metadataName) =>
            Type.GetAttributes().Any(a => string.Equals(a.AttributeClass?.ToDisplayString(), metadataName, StringComparison.Ordinal));

        /// <summary>True when the type name ends with <paramref name="suffix"/>.</summary>
        public bool NameEndsWith(string suffix) => Type.Name.EndsWith(suffix, StringComparison.Ordinal);

        /// <summary>Resolves the category folder per the precedence in §6.6.</summary>
        public string? ResolveCategory() => Shared.Policy.ResolveCategory(Type);

        /// <summary>Locates the structure this type's file falls under.</summary>
        public bool TryGetStructure(out StructureMatch structure, out string? actualCategoryFolder)
        {
            var match = Shared.Policy.MatchStructure(FilePath);
            if (match is null)
            {
                structure = null!;
                actualCategoryFolder = null;
                return false;
            }

            structure = match;
            actualCategoryFolder = match.ActualCategoryFolder;
            return true;
        }

        /// <summary>Reports a diagnostic on the type's identifier.</summary>
        public void Report(DiagnosticDescriptor descriptor, params object[] messageArgs)
        {
            var location = Type.Locations.FirstOrDefault(l => l.IsInSource) ?? Location.None;
            Report(descriptor, location, messageArgs);
        }

        /// <summary>Reports a diagnostic at the given location.</summary>
        public void Report(DiagnosticDescriptor descriptor, Location location, params object[] messageArgs) =>
            _report(Diagnostic.Create(descriptor, location, messageArgs));
    }
}
