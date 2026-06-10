using System;
using Microsoft.CodeAnalysis;

namespace Stricture
{
    /// <summary>A declared <c>TypeFolder</c>: a classification signal mapped to a category folder.</summary>
    internal sealed class TypeFolderPolicy
    {
        public TypeFolderPolicy(
            string folder,
            TypeShape kind,
            INamedTypeSymbol? derivesFrom,
            INamedTypeSymbol? implements,
            INamedTypeSymbol? markedWith,
            string? endsWith,
            bool isRecord,
            RuleTier tier)
        {
            Folder = folder;
            Kind = kind;
            DerivesFrom = derivesFrom;
            Implements = implements;
            MarkedWith = markedWith;
            EndsWith = endsWith;
            IsRecord = isRecord;
            Tier = tier;
        }

        public string Folder { get; }

        public TypeShape Kind { get; }

        public INamedTypeSymbol? DerivesFrom { get; }

        public INamedTypeSymbol? Implements { get; }

        public INamedTypeSymbol? MarkedWith { get; }

        public string? EndsWith { get; }

        public bool IsRecord { get; }

        public RuleTier Tier { get; }

        /// <summary>The tier, deriving from the signal when <see cref="RuleTier.Auto"/>.</summary>
        public RuleTier EffectiveTier
        {
            get
            {
                if (Tier != RuleTier.Auto)
                {
                    return Tier;
                }

                return (Kind != TypeShape.None || DerivesFrom != null)
                    ? RuleTier.Structural
                    : RuleTier.Convention;
            }
        }

        /// <summary>True when every specified signal matches <paramref name="type"/>.</summary>
        public bool Matches(INamedTypeSymbol type)
        {
            var any = false;

            if (Kind != TypeShape.None)
            {
                any = true;
                if (!MatchesKind(type, Kind))
                {
                    return false;
                }
            }

            if (DerivesFrom != null)
            {
                any = true;
                if (!DerivesFromSymbol(type, DerivesFrom))
                {
                    return false;
                }
            }

            if (Implements != null)
            {
                any = true;
                if (!ImplementsSymbol(type, Implements))
                {
                    return false;
                }
            }

            if (MarkedWith != null)
            {
                any = true;
                if (!HasAttributeSymbol(type, MarkedWith))
                {
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(EndsWith))
            {
                any = true;
                if (!type.Name.EndsWith(EndsWith!, StringComparison.Ordinal))
                {
                    return false;
                }
            }

            if (IsRecord)
            {
                any = true;
                if (!type.IsRecord)
                {
                    return false;
                }
            }

            return any;
        }

        private static bool MatchesKind(INamedTypeSymbol type, TypeShape kind)
        {
            switch (kind)
            {
                case TypeShape.Class: return type.TypeKind == TypeKind.Class && !type.IsRecord;
                case TypeShape.Struct: return type.TypeKind == TypeKind.Struct && !type.IsRecord;
                case TypeShape.Interface: return type.TypeKind == TypeKind.Interface;
                case TypeShape.Enum: return type.TypeKind == TypeKind.Enum;
                case TypeShape.Record: return type.IsRecord;
                case TypeShape.Delegate: return type.TypeKind == TypeKind.Delegate;
                default: return false;
            }
        }

        private static bool DerivesFromSymbol(INamedTypeSymbol type, INamedTypeSymbol baseType)
        {
            var current = type.BaseType;
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

        private static bool ImplementsSymbol(INamedTypeSymbol type, INamedTypeSymbol iface)
        {
            foreach (var i in type.AllInterfaces)
            {
                if (SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, iface.OriginalDefinition))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasAttributeSymbol(INamedTypeSymbol type, INamedTypeSymbol attr)
        {
            foreach (var a in type.GetAttributes())
            {
                if (a.AttributeClass != null
                    && SymbolEqualityComparer.Default.Equals(a.AttributeClass.OriginalDefinition, attr.OriginalDefinition))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
