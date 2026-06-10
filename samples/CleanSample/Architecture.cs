using System;
using Stricture;

// A realistic policy: two folder structures, classification rules, co-location, default visibility, a package ban.
[assembly: FolderStructure("Features", Pattern = "{feature}/{category}", Fallback = "Models")]
[assembly: FolderStructure("Platform", Pattern = "{area}/{category}", Fallback = "Internal")]

[assembly: TypeFolder("Enumerations", Kind = TypeShape.Enum)]            // structural
[assembly: TypeFolder("Exceptions", DerivesFrom = typeof(Exception))]   // structural
[assembly: TypeFolder("Requests", EndsWith = "Request")]                // convention
[assembly: TypeFolder("Requests", EndsWith = "Response")]               // same folder → co-location is legal
[assembly: TypeFolder("Models", IsRecord = true)]                       // convention

[assembly: OneTypePerFile]
[assembly: CoLocateBySuffix("Request", "Response")]

[assembly: DefaultVisibility(Visibility.Internal)]
[assembly: ForbidInterfaceNaming]

// Severity defaults to Warning; opt a single rule up to a build error with Severity = Severity.Error.
[assembly: BanPackage("Moq", Message = "Hand-write test doubles instead.", Severity = Severity.Error)]

// IServiceCollection extensions all live in one partial ServiceCollectionExtensions class.
[assembly: ExtensionMethodHome(
    typeof(Microsoft.Extensions.DependencyInjection.IServiceCollection),
    "ServiceCollectionExtensions",
    Namespace = "Microsoft.Extensions.DependencyInjection")]
