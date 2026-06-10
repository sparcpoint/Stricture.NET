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
[assembly: BanPackage("Moq", Message = "Hand-write test doubles instead.")]
