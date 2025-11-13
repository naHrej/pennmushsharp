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

## Command Catalog
- Run `dotnet run --project tools/CommandTableExtractor` to regenerate
  `docs/generated/commands.json`.
- The extractor inspects `pennmush/src/command.c` (and associated headers) to capture
  the canonical command metadata, including type flags, required switches, and privilege hints.
  `PennMushSharp.Commands.Metadata.CommandMetadataCatalog` loads this JSON so the new parser and
  permission checks stay aligned with the upstream tables. Custom/extended commands can register
  overrides at runtime, so we can diverge when needed without editing the generated snapshot.

## Modernization Notes (Backward-Compatible Goals)
- **Evaluator Registers:** Legacy `%0-%9`/`%q0-%qz` slots were artifacts of fixed-size arrays. The
  new evaluator lifts these limits: `%10+` resolves automatically, and `%q` registers are keyed by
  arbitrary identifiers (`setq(foo,bar)` -> `%qfoo`). Classic names still work, so softcode written
  for PennMUSH remains valid while modern code can name registers explicitly.

- **Switch Flexibility:** Canonical switch names remain uppercase tokens sourced from the metadata,
  but the dispatcher now supports metadata overrides so native commands/functions can opt into
  richer validation or alias sets without editing the C tables. This keeps compliance while allowing
  new commands to grow beyond the legacy model.

- **Permission Model:** Flags/powers from `command.c` are honored, yet PennMushSharp layers on
  extensible policies (role configs, ACL hooks) without changing existing behavior. Commands with no
  metadata fall back to legacy-style execution, so local extensions can coexist with upstream data.

- **Output Formatting:** Traditional telnet text stays the default, but the runtime is designed to
  emit optional HTML5/CSS fragments for modern clients. A future goal is per-command output templates
  (configurable in-game) that can produce both classic text and rich markup simultaneously.

- **Queues/Async:** The managed scheduler lifts command queue limits imposed by PennMUSH’s C structure
  while respecting its throttling semantics. Modern features (async tasks, larger queues) can be
  configured without breaking scripts that rely on the original behavior.

- **Persistence:** Text dump compatibility is maintained, but the in-memory model can serialize to
  other stores (JSON, SQL, object databases) so modern deployments don’t have to rely on legacy
  formats. Import/export of PennMUSH dumps remains available for interoperability.

- **Hooks/Plugins:** Legacy `@hook`s still fire, yet the runtime can expose managed extension points
  (events, DI services) for admins who want to plug in .NET code. When no managed hook is registered
  the system behaves exactly like PennMUSH.
