# PennMushSharp

PennMushSharp is a ground-up C# port of the PennMUSH server plus the Aspace
spaceflight extension. The repo currently contains scaffolding projects and a
roadmap to guide the 1:1 parity effort. See `ROADMAP.md` for phase planning.

## Tooling
- `tools/FlagTableExtractor`: pulls the canonical flag/power definitions directly from
  the upstream `pennmush` headers and writes `docs/generated/flags.json`. Run with
  `dotnet run --project tools/FlagTableExtractor`. The result is embedded into
  `PennMushSharp.Core` via `FlagCatalog` so gameplay code can consume the exact
  metadata the C server exposes.
- `tools/AttributeTableExtractor`: parses `atr_tab.h`/`attrib.h` and emits
  `docs/generated/attributes.json`, which the runtime loads through
  `PennMushSharp.Core.Attributes.AttributeCatalog` for attribute/alias parity.
- `tools/LockTableExtractor`: converts `lock_tab.h` into `docs/generated/locks.json`,
  powering `PennMushSharp.Core.Locks.LockCatalog`.
- `tools/FunctionTableExtractor`: reads `function.c` to produce
  `docs/generated/functions.json`, consumed by `PennMushSharp.Core.Functions.FunctionCatalog`.
- `scripts/characterize.sh`: drives the legacy PennMUSH binary using scenario files
  under `tests/characterization/scenarios/` to capture golden transcripts.
- `scripts/fetch-upstreams.sh`: clones/updates the upstream C sources (`pennmush` and
  `aspace`) into gitignored folders for local development.
- `scripts/regenerate-metadata.sh`: convenience wrapper that runs every extractor and
  refreshes all JSON snapshots in `docs/generated/`.

## Characterization Workflow
- Run `scripts/characterize.sh --scenario tests/characterization/scenarios/<name>.scenario --golden`
  to capture a transcript and automatically refresh `tests/characterization/golden/<name>.log`.
- The harness writes ad-hoc captures to `tests/characterization/output/` and can be pointed at a
  different golden directory with `--golden-dir`.
- `CharacterizationGoldenTests` (under `PennMushSharp.Tests`) validate that every scenario has a
  corresponding golden log and that each transcript carries the expected metadata banner. This
  keeps the parity fixtures in lockstep with the scenarios we depend on for regression coverage.
- When the legacy C binary is unavailable, rely on the checked-in stock dumps and metadata catalogs;
  the new persistence layer (see below) lets us continue porting without needing to execute the
  upstream server for every change.
- `scripts/start-legacy-mush.sh` resets the stock PennMUSH database (currently seeded with
  `Wizard9` / `harness`) and launches `pennmush/game/netmush` so the harness can reproduce the exact
  same world state before every transcript capture.

## Persistence & Metadata Ingestion
- `PennMushSharp.Core.Persistence.TextDumpParser` understands both the legacy `@dump` output and the
  modern structured format (the one produced by the 1.8.x stock DB). It extracts dbrefs, owners,
  flags, locks, and arbitrary attributes into `GameObjectRecord`.
- `InMemoryGameState` consumes those records and wires the parsed lock expressions into the runtime’s
  `InMemoryLockStore`, so subsystems like `LockEvaluator` can immediately execute metadata-driven
  permission checks.
- This pipeline forms the basis of the eventual database adapter: once we finalize serialization,
  the runtime can boot entirely from archived dumps without needing the original C process on hand.

## Runtime Metadata Access
`PennMushSharp.Core.Metadata.MetadataCatalogs` exposes singleton access to the
flag/attribute/lock/function catalogs so upcoming subsystems (locks, commands, function
evaluation, persistence) can share the same canonical data. `LockMetadataService`,
`LockEvaluator`, `SimpleLockExpressionEngine`, and the new `InMemoryGameState`/`InMemoryLockStore`
pipeline show how metadata flows into runtime behaviour while we continue fleshing out full
persistence.

## Runtime Host & Commands
- `PennMushSharp.Runtime` now composes the DI container via `RuntimeApplication` and boots through
  `DefaultServerBootstrapper`, which can optionally load an initial dump (`PennMushSharp:InitialDumpPath`)
  before exposing services like `CommandDispatcher`.
- `CommandCatalog` / `LookCommand` act as the first slice of the command subsystem; the dispatcher
  resolves commands by name and executes them asynchronously so higher-level telnet/web sockets can
  plug in without rewriting the pipeline.
- `TelnetServer` (hosted background service) listens on `PennMushSharp:ListenAddress/ListenPort`
  (defaults: `127.0.0.1:4201`) and registers live sessions so commands like `WHO` can report active
  connections. New sessions log in via `CONNECT <name> <password>` (seeded with `Wizard9` /
  `harness` via `GameStateSeeder`) or create an account with `CREATE <name> <password>` before
  issuing managed commands. Newly created accounts are persisted as PennMUSH-compatible dumps
  (default path `PennMushSharp/data/accounts.dump`, override via `PennMushSharp:AccountStorePath`).

## External Dependencies
The `pennmush/` and `aspace/` directories live at the root of this repo but are ignored by
git. Fetch or update them with:

```bash
cd PennMushSharp
scripts/fetch-upstreams.sh
# or pin refs
PENNMUSH_REF=1.8.9p2 ASPACE_REF=main scripts/fetch-upstreams.sh
```

By default the script clones `https://github.com/pennmush/pennmush` and
`https://github.com/aspace-sim/aspace`. Override `PENNMUSH_REPO`, `PENNMUSH_REF`,
`ASPACE_REPO`, or `ASPACE_REF` as needed.
