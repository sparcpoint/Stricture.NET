---
name: bump-version
description: Bump the Stricture.Net package version from git history and update the NuGet release notes. Use when the user wants to cut a release, bump the version, or refresh release notes (e.g. "/bump-version", "bump the version", "prep a release"). Analyzes commits since the last release, recommends a semver bump, and updates the centralized version + PackageReleaseNotes.
---

# /bump-version — version bump + release notes from git

Decide the next version from what changed in git, then update the **single** version source and the
NuGet **release notes**. Version is centralized in [`Directory.Build.props`](../../../Directory.Build.props)
(`<Version>`); the package metadata (`<PackageReleaseNotes>`) lives in
[`src/Stricture.Package/Stricture.Package.csproj`](../../../src/Stricture.Package/Stricture.Package.csproj).
Read [CLAUDE.md](../../../CLAUDE.md) for repo context.

## Procedure

### 1. Find the current version and the baseline
- Current version = the `<Version>` in `Directory.Build.props`.
- Baseline (what "since last release" means), in order of preference:
  1. The latest semver tag: `git describe --tags --abbrev=0 --match "v*"`.
  2. Else the commit that last changed `<Version>` in `Directory.Build.props`
     (`git log -1 --format=%H -L` is awkward — instead `git log --oneline -- Directory.Build.props` and
     pick the most recent version-bump commit).
  3. Else the root commit.
- If the baseline is ambiguous, ask the user which ref to compare against.

### 2. Gather the changes
Run `git log <baseline>..HEAD --oneline` and `git diff <baseline>..HEAD --stat` (and read key diffs as
needed). Also account for **uncommitted** working-tree changes — they're part of this release if the user
intends to commit them. Summarize what actually changed (new rules/attributes, fixes, refactors, docs,
packaging).

### 3. Classify the semver bump (project-specific)
- **MAJOR** — a breaking change for consumers: removing/renaming a public attribute or its members,
  changing `WellKnownNames.Ns`, removing/renaming a diagnostic id, raising a rule's default severity to
  `error`, changing target frameworks or the package id.
- **MINOR** — backward-compatible additions: a new attribute, a new rule / `ARCH` diagnostic id, new
  opt-in capability. (New analyzers default to `Warning`, so surfacing new warnings is additive, not
  breaking.)
- **PATCH** — bug/false-positive fixes in rule logic, docs, tests, packaging, internal refactors (e.g.
  file reorganization).

Pick the highest level any change warrants. Compute the new version from the current one
(`MAJOR` → `2.0.0`, `MINOR` → `1.(n+1).0`, `PATCH` → `1.0.(n+1)`).

### 4. Draft release notes
Write a concise entry in the existing style — `"<version> — <one-line summary>. <details>."` — covering the
notable user-facing changes (group new rules/attributes, fixes, etc.). Keep it changelog-quality, not a
raw commit dump.

### 5. Confirm (AskUserQuestion)
Show the detected changes, the recommended bump level (pre-selected) with major/minor/patch options, the
resulting version, and the drafted notes. Let the user override the level or edit the notes before
applying.

### 6. Apply
- Set `<Version>` in `Directory.Build.props` to the new version. (Do **not** add versions to the other
  projects — they inherit; that's the whole point of centralizing.)
- Update `<PackageReleaseNotes>` in `Stricture.Package.csproj`: **prepend** the new entry so notes read
  newest-first (changelog style), keeping prior entries.

### 7. Report & next steps
- Remind the user that **building doesn't pack** (`GeneratePackageOnBuild=False`), and that because the
  package embeds the DLLs by file path, they must **Rebuild → Pack** (not just Build → Pack) so the
  embedded `Stricture.Abstractions.dll` / `Stricture.Engine.dll` carry the new version. CLI equivalent:
  `dotnet build -c Release` then `dotnet pack -c Release`, followed by `bash scripts/verify-package.sh`.
- Do **not** commit, tag, or push unless asked. Offer to commit the bump (and to tag `v<new>`) if they
  want.

## Notes
- Version flows from the single `<Version>` to PackageVersion, AssemblyVersion, and FileVersion — don't
  hand-edit those separately.
- If nothing meaningful changed since the baseline, say so and recommend not bumping.
