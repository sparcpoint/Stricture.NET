# Stricture.Net

> **Architectural rules for .NET that break the build.**
>
> Declare your conventions as assembly attributes; violations show up as live IDE squiggles
> and `dotnet build` errors. A standalone analyzer toolkit — not a client library for another service.

## Install

```
dotnet add package Stricture.Net
```

One `<PackageReference Include="Stricture.Net" />` delivers both assets: the attribute library you
reference (`lib/netstandard2.0/Stricture.Abstractions.dll`) and the Roslyn analyzer that enforces it
(`analyzers/dotnet/cs/Stricture.Engine.dll`). The consumer writes `using Stricture;`.

## Quick start

Drop an `Architecture.cs` in the project you want to govern:

```csharp
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
[assembly: BanPackage("Moq", Message = "Use hand-written test doubles.")]
```

Adding a *fact* is one attribute instance (no code). Everything below is enforced at build time and in
the IDE. All rules default to **Warning** — see [Severity](#severity) to turn them into errors.

## How classification works

A **`FolderStructure`** declares a root folder and a path template. The `{category}` token is the slot
that classification rules map into; any other `{token}` matches a single path segment. `Fallback` is the
category folder used for types the structure can't otherwise classify.

A **`TypeFolder`** maps a *signal* (a fact about a type) to a *category folder*. Signals come in two
tiers:

| Signal | Example | Tier (when `Tier = Auto`) |
|--------|---------|---------------------------|
| `Kind` | `Kind = TypeShape.Enum` | **Structural** |
| `DerivesFrom` | `DerivesFrom = typeof(Exception)` | **Structural** |
| `Implements` | `Implements = typeof(IHandler)` | Convention |
| `MarkedWith` | `MarkedWith = typeof(MyAttribute)` | Convention |
| `EndsWith` | `EndsWith = "Request"` | Convention |
| `IsRecord` | `IsRecord = true` | Convention |

When several `TypeFolder`s match a type, the category is resolved by precedence: **structural matches win
over convention matches; within a tier, the last-declared wins**; if nothing matches, the structure's
`Fallback` is used; if there's no fallback, the type is ignored. You can override a signal's tier by
setting `Tier = RuleTier.Structural` (or `RuleTier.Convention`) on the `TypeFolder`.

### Example layout

With the policy above, this tree builds clean:

```
Features/
  Payments/                         {feature} = Payments
    Models/         Payment.cs      {category} = Models      (record → Models)
    Enumerations/   PaymentStatus.cs                         (enum → Enumerations)
    Exceptions/     PaymentFailedException.cs                (: Exception → Exceptions)
    Requests/       CreateOrder.cs                           (…Request/…Response, co-located)
Platform/
  Logging/
    Internal/       LogScope.cs     {category} = Internal    (unclassified → Fallback)
```

Move `PaymentStatus.cs` into `Models/` and you get:

```
ARCH1001: Type 'PaymentStatus' should be in folder 'Enumerations' but is in 'Models'
```

## Examples by rule

### Folder placement — `ARCH1001` / `ARCH1002`

```csharp
[assembly: FolderStructure("Features", Pattern = "{feature}/{category}", Fallback = "Models")]
[assembly: TypeFolder("Handlers", EndsWith = "Handler")]
```

- `ARCH1001` — a top-level type sits in the wrong category folder (expected = its resolved category, or
  the structure `Fallback`).
- `ARCH1002` — a type lives under a structure root but its path depth doesn't match the pattern (e.g.
  `Features/Foo.cs` when the pattern expects `Features/{feature}/{category}/Foo.cs`).

### Promote nested types — `ARCH1003`

A `public`/`internal` nested type that classifies to a category should be its own file (private/protected
nested types are ignored):

```csharp
public class Orders            // ARCH1003: 'Status' is a nested enum; promote it to its own file in 'Enumerations'
{
    public enum Status { Open, Closed }
}
```

### Don't name a type after its interface — `ARCH1010`

Always on, no configuration. A concrete `Foo : IFoo` is flagged — pick an intent-revealing name:

```csharp
public interface IClock { }
public sealed class Clock : IClock { }       // ARCH1010 — rename to SystemClock, FixedClock, …
```

### Internal by default — `ARCH1020`

```csharp
[assembly: DefaultVisibility(Visibility.Internal)]

public sealed class OrderService { }          // ARCH1020 — should be internal
[PublicApi] public sealed class OrderClient { }   // exempt: intentional public surface
```

### One type per file & co-location — `ARCH2001` / `ARCH2002`

```csharp
[assembly: OneTypePerFile]                                  // RequireSharedStem = true by default
[assembly: CoLocateBySuffix("Request", "Response")]         // these suffixes may share a file
```

```csharp
// CreateOrder.cs — OK: one stem, suffixes in the same co-location group
public sealed class CreateOrderRequest { }
public sealed class CreateOrderResponse { }

// Mixed.cs — ARCH2002: co-located types must share a common stem
public sealed class CreateOrderRequest { }
public sealed class DeleteOrderResponse { }

// TwoThings.cs — ARCH2001: unrelated types; 'Helper' should move to its own file
public sealed class Order { }
public sealed class Helper { }
```

### Ban types, namespaces, and packages — `ARCH3001` / `ARCH4001`

```csharp
[assembly: BanType(Type = typeof(System.Net.WebClient), Message = "Use HttpClient via IHttpClientFactory.")]
[assembly: BanType(FullyQualifiedName = "System.DateTime", Message = "Use DateTimeOffset.")]
[assembly: BanNamespace("System.Data.SqlClient", Message = "Use Microsoft.Data.SqlClient.")]
[assembly: BanPackage("Newtonsoft.Json", Message = "Standardize on System.Text.Json.")]
```

- `ARCH3001` — usage of a banned type, or any type in a banned namespace (and sub-namespaces). The
  attribute's `Message` is included in the diagnostic.
- `ARCH4001` — the compilation references a banned assembly (≈ a banned package).

## Severity

Every `ARCHxxxx` rule defaults to **Warning** so consumer builds stay green until you opt in. Escalate any
rule — or a whole category — to a build error in `.editorconfig`:

```ini
[*.cs]
dotnet_diagnostic.ARCH1001.severity = error      # one rule
dotnet_analyzer_diagnostic.category-Stricture.Layout.severity = error   # a whole category
```

Categories: `Stricture.Config`, `Stricture.Engine`, `Stricture.Layout`, `Stricture.Naming`,
`Stricture.Visibility`, `Stricture.Bans`.

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
