# CLAUDE.md

Guidance for AI agents working in this repository. Keep it current when structure or invariants change.

## What this is

**Stricture.Net** is an attribute-driven Roslyn analyzer toolkit. A consuming project declares its
architectural conventions as **assembly-level attributes** (folder layout, type classification, file
co-location, default visibility, banned types/namespaces/packages), and the analyzer reports violations
as live IDE squiggles and `dotnet build` diagnostics (`ARCHxxxx`).

It is a standalone analyzer — **not** a client library for any service. Scope is **per-assembly
(single-compilation) only**; cross-assembly/solution aggregation is explicitly out of scope.

The consumer experience: install one package, write `using Stricture;`, apply attributes in an
`Architecture.cs`. See [README.md](README.md) for the usage example.

## Solution layout

```
src/
  Stricture.Abstractions/   netstandard2.0 — the attributes the CONSUMER references (no Roslyn dep)
  Stricture.Engine/         netstandard2.0 — the analyzer (references Roslyn, never references Abstractions)
  Stricture.Package/        packaging-only project; assembles the single Stricture.Net NuGet
tests/
  Stricture.Tests/          net10.0 — xUnit (logic + integration + guards)
samples/
  CleanSample/              net10.0 — references the engine; MUST build with zero ARCH diagnostics
```

Engine and Abstractions are `IsPackable=false`; **only `Stricture.Package` packs**. It embeds
`Stricture.Abstractions.dll` into `lib/netstandard2.0` and `Stricture.Engine.dll` into
`analyzers/dotnet/cs`, referencing both projects with `ReferenceOutputAssembly=false` (build order only —
neither is a compile dependency of the package). The engine does not reference Abstractions; it matches
the consumer's attributes by metadata-name string.

## Invariants (do not break)

- **`WellKnownNames.Ns` MUST equal `"Stricture"`.** The engine finds attributes by string name; if the
  namespace drifts, the analyzer silently finds nothing. A guard test enforces this, plus that every
  `WellKnownNames` constant equals the real `typeof(XxxAttribute).FullName`.
- **Package id is `Stricture.Net`; assembly names and the root namespace stay `Stricture`.** Consumers
  write `using Stricture;`.
- **Engine targets `netstandard2.0`** and must load in the compiler/VS. Avoid APIs unavailable there:
  no `Path.GetRelativePath` (use [`PathUtil`](src/Stricture.Engine/Util/PathUtil.cs)), no
  `string.Contains(char)`/`Contains(string, StringComparison)` (use `IndexOf(..., StringComparison)`).
  **No runtime dependencies beyond Roslyn** — config is attribute-based so the analyzer needs no parser.
- **Every diagnostic id must be listed in
  [`AnalyzerReleases.Unshipped.md`](src/Stricture.Engine/AnalyzerReleases.Unshipped.md)** or the engine
  build fails (RS2007/RS2008; `EnforceExtendedAnalyzerRules=true`).
- **`samples/CleanSample` must build with zero diagnostics** (it's in the solution build; with
  `TreatWarningsAsErrors` any false positive fails CI). Intentional violations live only as **test
  fixtures**, never as a buildable project.
- **No Moq / no mocking library** in tests.

## Engine architecture

One root `[DiagnosticAnalyzer]`, [`StrictureAnalyzer`](src/Stricture.Engine/StrictureAnalyzer.cs):

- Discovers all rules in the engine assembly by **reflection** (non-abstract subclasses of the rule base
  classes), instantiates once. Adding a rule needs no registration — just create the class.
- Builds a [`SharedContext`](src/Stricture.Engine/Context/SharedContext.cs) **once** per compilation in
  `CompilationStartAction` (parses the policy from `compilation.Assembly.GetAttributes()`, resolves
  symbols, validates config). Each rule gets a cheap per-element context wrapping it.
- Dispatches to scoped rule families, each overriding one `Analyze` method:
  - `TypeRule` → `RegisterSymbolAction(NamedType)`
  - `FileRule` → `RegisterSyntaxTreeAction` (syntactic; skips generated code)
  - `OperationRule` → `RegisterOperationAction`
  - `CompilationRule` → `RegisterCompilationEndAction` (config validation + assembly bans)
- **Per-rule isolation:** every `Analyze` runs in try/catch; a throw reports `ARCH0002` instead of
  killing analysis. `EnableConcurrentExecution`, generated code skipped.

Policy parsing and classification live in [`Model/`](src/Stricture.Engine/Model). Classification
precedence (`Policy.ResolveCategory`): structural-tier matches first (last declared wins), then
convention-tier (last wins), else null → caller applies the structure `Fallback`.

## Adding a rule

1. Create `src/Stricture.Engine/Rules/<Name>Rule.cs` subclassing the right base; expose its
   `DiagnosticDescriptor`(s) and implement `Analyze`. (Reflection discovers it automatically.)
2. Add the descriptor in [`Diagnostics/Descriptors.cs`](src/Stricture.Engine/Diagnostics/Descriptors.cs).
   Default severity `Warning`. Watch RS1032: single-sentence messages take **no** trailing period.
3. Add the id row to
   [`AnalyzerReleases.Unshipped.md`](src/Stricture.Engine/AnalyzerReleases.Unshipped.md).
4. Add `tests/Stricture.Tests/Rules/<Name>RuleTests.cs` with both a logic and an integration fixture
   (≥1 passing + ≥1 failing per id).
5. If the rule needs a new consumer attribute, add it to `Stricture.Abstractions`, add a `WellKnownNames`
   constant, and update the name-sync guard's mapping list.
6. Confirm `CleanSample` still builds clean.

## Diagnostics

`ARCH0001` config validation · `ARCH0002` rule-error isolation · `ARCH1001` wrong category folder ·
`ARCH1002` path doesn't match structure pattern · `ARCH1003` nested-type promotion · `ARCH1010`
named-after-interface (`Foo : IFoo`) · `ARCH1020` internal-by-default · `ARCH2001` one-type-per-file ·
`ARCH2002` co-location suffix/stem · `ARCH3001` banned type/namespace usage · `ARCH4001` banned assembly.

## Testing

Two layers, both xUnit (`tests/Stricture.Tests`, organized one file per rule under `Rules/`):

- **Logic tests** — fast, no MSBuild. [`RuleTestHarness`](tests/Stricture.Tests/Infrastructure/RuleTestHarness.cs)
  parses a compilation with an **explicit file path** (`CSharpSyntaxTree.ParseText(src, path: ...)`) and
  runs a single rule. Setting the path is what makes folder rules deterministic.
- **Integration tests** — `Microsoft.CodeAnalysis.CSharp.Analyzer.Testing.XUnit` against the root
  analyzer via [`StrictureAnalyzerTest`](tests/Stricture.Tests/Infrastructure/StrictureAnalyzerTest.cs)
  (`ReferenceAssemblies.Net.Net80`). Failing fixtures use `{|ARCHxxxx:span|}` markup; no-location
  diagnostics use explicit `DiagnosticResult().WithNoLocation()`.
- **Guards** (`Guards/`): name-sync, descriptor-uniqueness + release-tracking coverage, and the
  no-false-positive guard. `ARCH0002` is covered by the isolation tests in `Engine/`.

## Build & verify

```
dotnet build -c Release        # 0 warnings (TreatWarningsAsErrors); CleanSample proves no false positives
dotnet test  -c Release        # all green
dotnet pack  -c Release        # produces exactly one package: Stricture.Net
bash scripts/verify-package.sh # asserts both assets are at the correct paths
```

The .NET 10 SDK is pinned in [`global.json`](global.json) (10.0.300). [`nuget.config`](nuget.config)
pins a single feed (nuget.org) so CPM doesn't trip NU1507. Open the solution in **VS 2026** — VS 2022
cannot load the .NET 10 SDK.

## Conventions

- **One top-level type per file**, organized into folders by concern (the repo dogfoods its own
  `ARCH2001`/`ARCH1003`). Folders do **not** map to namespaces — the public authoring surface is the
  single `Stricture` namespace, so `IDE0130` (namespace-matches-folder) is disabled in `.editorconfig`.
- Block-bodied namespaces; `ImplicitUsings` disabled.
- **`IDE0005` is an error** (unused usings) — include only the usings a file actually needs.
- Public API on shipped libraries (Abstractions/Engine) needs XML docs (`GenerateDocumentationFile`,
  CS1591). The test project suppresses `CS1591`/`CA1707`.
- Central Package Management ([`Directory.Packages.props`](Directory.Packages.props)); engine baseline is
  `Microsoft.CodeAnalysis.CSharp` 4.8.0 (broad VS compatibility — don't bump past the target VS).
- Commit/push only when asked. Co-author trailer: `Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>`.
