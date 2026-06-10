; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|------
ARCH0001 | Stricture.Config | Warning | Invalid or contradictory configuration
ARCH0002 | Stricture.Engine | Warning | Internal rule error
ARCH1001 | Stricture.Layout | Warning | Type in wrong category folder
ARCH1002 | Stricture.Layout | Warning | Type path does not match structure pattern
ARCH1003 | Stricture.Layout | Warning | Nested type should be promoted to its own file
ARCH1010 | Stricture.Naming | Warning | Concrete type named after its interface
ARCH1020 | Stricture.Visibility | Warning | Public type should be internal by default
ARCH2001 | Stricture.Layout | Warning | More than one top-level type per file
ARCH2002 | Stricture.Layout | Warning | Co-located types violate suffix or stem rules
ARCH3001 | Stricture.Bans | Warning | Usage of a banned type or namespace
ARCH4001 | Stricture.Bans | Warning | Reference to a banned assembly
