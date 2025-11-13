# Domain Model (WIP)

This document will capture the canonical definitions for PennMUSH dbrefs, object types,
flags, locks, and attribute tables. Initial extraction will come from the legacy
`pennmush/src/*.dst` data tables and will be kept in sync with generated C# sources.

## Flag & Power Catalog
- Run `dotnet run --project tools/FlagTableExtractor` from the repo root to regenerate
  `docs/generated/flags.json`.
- The generated artifact is sourced directly from `pennmush/hdrs/flag_tab.h`, ensuring the
  C# implementation always matches the upstream flag/power names, letters, bit positions,
  and permission masks. The runtime consumes this data via `PennMushSharp.Core.Flags.FlagCatalog`.

## Attribute Catalog
- Run `dotnet run --project tools/AttributeTableExtractor` to refresh
  `docs/generated/attributes.json`.
- The extractor reads `pennmush/hdrs/atr_tab.h` (plus `attrib.h`/`chunk.h`) to preserve
  attribute flags, default creators, chunk references, and aliases. These details are
  embedded into `PennMushSharp.Core.Attributes.AttributeCatalog` so higher-level code can
  mirror the original access rules.

## Lock Catalog
- Run `dotnet run --project tools/LockTableExtractor` to regenerate
  `docs/generated/locks.json`.
- The extractor captures default lock definitions and privilege metadata from
  `lock_tab.h`/`lock.h`. The runtime exposes this via `PennMushSharp.Core.Locks.LockCatalog`
  so permission checks and command behaviors can mirror the C server.

## Function Catalog
- Run `dotnet run --project tools/FunctionTableExtractor` to rebuild
  `docs/generated/functions.json`.
- The extractor parses `src/function.c`, recording builtin functions, min/max argument
  counts, parser flags, and alias relationships. `PennMushSharp.Core.Functions.FunctionCatalog`
  consumes the snapshot to drive the future function evaluator.
