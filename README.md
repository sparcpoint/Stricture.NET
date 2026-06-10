# Stricture.Net

> **Architectural rules for .NET that break the build.**
>
> Declare your conventions as assembly attributes; violations show up as live IDE squiggles
> and `dotnet build` errors. A standalone analyzer toolkit — not a client library for another service.

Install:

```
dotnet add package Stricture.Net
```

One `<PackageReference Include="Stricture.Net" />` delivers both assets: the attribute library you
reference (`lib/netstandard2.0/Stricture.Abstractions.dll`) and the Roslyn analyzer that enforces it
(`analyzers/dotnet/cs/Stricture.Engine.dll`). The consumer writes `using Stricture;`.

## Usage

```csharp
// Architecture.cs in the consuming project
using Stricture;

[assembly: FolderStructure("Features", Pattern = "{feature}/{category}", Fallback = "Models")]
[assembly: FolderStructure("Platform", Pattern = "{area}/{category}",    Fallback = "Internal")]

[assembly: TypeFolder("Enumerations", Kind = TypeShape.Enum)]            // structural
[assembly: TypeFolder("Exceptions",   DerivesFrom = typeof(System.Exception))]
[assembly: TypeFolder("Requests",     EndsWith = "Request")]            // convention
[assembly: TypeFolder("Requests",     EndsWith = "Response")]           // same folder → co-location is legal
[assembly: TypeFolder("Models",       IsRecord = true)]

[assembly: OneTypePerFile]
[assembly: CoLocateBySuffix("Request", "Response")]

[assembly: DefaultVisibility(Visibility.Internal)]
[assembly: BanPackage("Moq", Message = "Use NSubstitute-free hand-written test doubles.")]
```

Adding a *fact* is one attribute instance (no code); adding a *behavior* is one small rule class in the
engine. Rules are pure functions of a context object, so each is testable in isolation.

## Severity

Every `ARCHxxxx` rule defaults to **Warning** so consumer builds stay green until you opt in. Escalate
any rule to a build error in `.editorconfig`:

```ini
[*.cs]
dotnet_diagnostic.ARCH1001.severity = error
```

## Diagnostics

| ID | What it catches |
|----|-----------------|
| `ARCH0001` | Invalid or contradictory configuration |
| `ARCH0002` | An internal rule error (isolated; never kills analysis) |
| `ARCH1001` | A top-level type in the wrong category folder |
| `ARCH1002` | A type whose path doesn't match its structure's pattern |
| `ARCH1003` | A public/internal nested type that should be promoted to its own file |
| `ARCH1010` | A concrete type named after its interface (`Foo : IFoo`) |
| `ARCH1020` | A public type that should be internal by default |
| `ARCH2001` | More than one top-level type per file |
| `ARCH2002` | Co-located types that mix suffix groups or have mismatched stems |
| `ARCH3001` | Usage of a banned type or namespace |
| `ARCH4001` | A reference to a banned assembly (package) |

## Scope

Per-assembly (single-compilation) rules only. Cross-assembly / whole-solution aggregation is out of
scope for this version.

## License

MIT — see [LICENSE](LICENSE).
