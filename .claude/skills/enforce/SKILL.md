---
name: enforce
description: Add or enforce an architectural principle in this Stricture.Net repo. Use when the user describes a convention to enforce, something to prevent, or gives an invalid example (e.g. "/enforce ...", "I want to forbid X", "types like Y should always Z"). Generalizes the principle, reuses existing attributes/rules where possible, and always adds tests for the user's specific case.
---

# /enforce — add an architectural rule to Stricture.Net

The user describes an architecture principle they want enforced — often by saying what to **prevent** or
giving an **invalid example**. Turn that into a reusable, tested enforcement in this codebase.

Read [CLAUDE.md](../../../CLAUDE.md) first — it has the invariants, layout, and the "Adding a rule"
checklist. Everything below assumes those rules (netstandard2.0 limits, `WellKnownNames.Ns == "Stricture"`,
CleanSample must stay clean, every diagnostic id in `AnalyzerReleases.Unshipped.md`, no Moq).

## Operating mode for this skill

- **Reuse-first.** Prefer expressing the principle with an existing attribute/rule, or by adding a new
  *attribute* that an existing rule already consumes. Only create a **new rule + new `ARCH` diagnostic id**
  when nothing existing can express it — and when that's required, call it out explicitly and get the
  user's go-ahead before writing engine code.
- **Plan + confirm.** Ask clarifying questions, generalize, then present a short plan and wait for the
  user's OK before implementing.
- **Always add tests** for the user's specific case — *unless* an equivalent test already exists, in which
  case say so and add nothing.
- **Dogfood:** add a compliant example to `samples/CleanSample` when it fits (must keep building clean).
- **Document:** add a usage example to `README.md` for any new attribute or principle.

## Procedure

### 1. Understand
Restate the principle in one sentence. Identify the **signal** (what makes a type/file/usage match) and
the **violation** (what should be reported). Capture the user's concrete example as a future test case.

### 2. Generalize
Broaden the principle so it's reusable by others, while keeping the user's example valid. E.g. "ban
`Newtonsoft.Json`" generalizes to the existing `BanPackage`/`BanNamespace`; "controllers must end in
`Controller`" generalizes to a suffix-classification or naming rule. Prefer parameters (suffix, base type,
namespace, marker attribute) over hard-coded names.

### 3. Classify — reuse vs. new
Inspect current capabilities (`src/Stricture.Abstractions/Attributes`, `src/Stricture.Engine/Rules`,
`src/Stricture.Engine/Diagnostics/Descriptors.cs`) and decide which case applies:

- **Already fully covered + already tested for this case** → tell the user it's supported, show how to
  configure it, add nothing.
- **Covered, but the user's case isn't tested** → add focused tests (and a CleanSample example if it
  fits). No engine changes.
- **A new *attribute* that an existing rule can consume** → add the attribute + policy parsing + tests.
  No new diagnostic id.
- **Needs new enforcement logic** → a new rule + new `ARCH` id is required. **Pause, state this clearly,
  and get explicit approval** before proceeding.

### 4. Clarify (AskUserQuestion)
Ask only what changes the outcome — at most ~4 questions. Good candidates: the matching signal (name
suffix / base type / implemented interface / marker attribute / language kind / namespace / assembly),
how far to generalize, the attribute name, severity default (normally `Warning`), and assembly-wide vs.
scoped. Skip anything you can infer confidently.

### 5. Plan + confirm
Present a concise plan: the attribute shape (name, target, members), whether a new rule/diagnostic id is
needed (and which `ARCH` number), the tests you'll add (naming the user's case), and any CleanSample/README
updates. If a new rule/diagnostic is involved, make that prominent. Wait for confirmation.

### 6. Implement
Follow the "Adding a rule" checklist in CLAUDE.md. Depending on the plan:

- **New attribute** → `src/Stricture.Abstractions/Attributes/<Name>Attribute.cs` (sealed, XML-documented,
  correct `AttributeUsage`/`AllowMultiple`). Add a `WellKnownNames` constant in
  `src/Stricture.Engine/Naming/WellKnownNames.cs` and a mapping row to the name-sync guard
  (`tests/Stricture.Tests/Guards/NameSyncGuardTests.cs`). Parse it in
  `src/Stricture.Engine/Context/SharedContext.cs` into a `Model/` type.
- **New rule** → `src/Stricture.Engine/Rules/<Name>Rule.cs` subclassing the right base
  (`TypeRule`/`FileRule`/`OperationRule`/`CompilationRule`); reflection discovers it automatically. Add the
  descriptor to `src/Stricture.Engine/Diagnostics/Descriptors.cs` (severity `Warning`; **RS1032**:
  single-sentence messages take no trailing period) and the id row to
  `src/Stricture.Engine/AnalyzerReleases.Unshipped.md`. Pick the next free id in the matching range:
  `0xxx` config/engine · `1xxx` type (layout/naming/visibility) · `2xxx` file · `3xxx` operation/usage ·
  `4xxx` compilation/reference.
- **Tests** (always) → `tests/Stricture.Tests/Rules/<Name>RuleTests.cs`. Include a fast logic test via
  `RuleTestHarness` and an integration test via `StrictureAnalyzerTest.WithSource(path, source)` with
  `{|ARCHxxxx:span|}` markup; cover ≥1 passing and ≥1 failing fixture, and explicitly the user's example.
  (Operation-scoped rules: integration only.)
- **CleanSample** → add a compliant example under `samples/CleanSample/...` when it fits, and the policy
  attribute to `samples/CleanSample/Architecture.cs`. It must still build with zero diagnostics.
- **README** → add a usage snippet for the new attribute/principle in the appropriate section.

### 7. Verify
Run and do not finish until green:
```
dotnet build -c Release    # 0 warnings
dotnet test  -c Release    # all pass
```
If you touched packaging, also `dotnet pack -c Release` + `bash scripts/verify-package.sh`. Then summarize
what changed: the attribute/rule, the diagnostic id (if any), the tests added, and the docs/sample updates.

## Notes
- One top-level type per file; minimal `using`s (`IDE0005` is an error); block-bodied namespaces; XML docs
  on public Abstractions/Engine API.
- Keep the engine dependency-free beyond Roslyn and free of netstandard2.0-unavailable APIs.
- Commit/push only if the user asks.
