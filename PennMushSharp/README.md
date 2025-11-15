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
- `tools/CommandTableExtractor`: parses `src/command.c` to capture the canonical command metadata
  (type flags, switch lists) into `docs/generated/commands.json`. The runtime loads this snapshot so
  the command parser/permissions stay aligned with upstream PennMUSH.
- PennMushSharp’s function evaluator will preserve PennMUSH semantics but lifts legacy limits:
  `%q` registers become string-keyed (`setq(foo,bar)` -> `%qfoo`), and command arguments are no longer
  capped at ten (`%10`, `%11`, etc. resolve when provided). Classic names (`%q0-%qz`, `%0-%9`) continue to
  work for compatibility.
- The builtin function catalog is now metadata-driven. `FunctionRegistryBuilder` consumes the generated
  `functions.json` snapshot, enforces PennMUSH min/max arity, and wires aliases so adding a new `IFunction`
  automatically lights up every legacy spelling. Initial implementations cover the register helpers
  (`SETQ`, `SETR`), core math (`ADD`, `SUB`, `MUL`, `DIV`, `MOD`, `ABS`, `MIN`, `MAX`, `CEIL`, `FLOOR`, `ROUND`, `ROOT`),
  trig/angle conversion (`SIN`, `COS`, `TAN`, `ASIN`, `ACOS`, `ATAN`, `ATAN2`, `CTU`), logarithmic helpers (`LOG`, `LN`, `PI`, `POWER`, `RAND`),
  and string helpers (`UPCASE`, `DOWNCASE`, `STRLEN`, `TRIM`, `LTRIM`, `RTRIM`, `LEFT`, `RIGHT`, `MID`, `REPEAT`).
- Output formatting keeps the plain-text telnet stream but plans for opt-in HTML5/CSS templates per
  command so modern clients can render richer UI without breaking legacy users. These templates will
  be configurable in-game once the telemetry/publishing pipeline is in place.
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
- `scripts/start-legacy-mush.sh` resets the stock PennMUSH database (seeded with the wizard account
  `One` / _no password_) and launches `pennmush/game/netmush` so the harness can reproduce the exact
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
- Binary dump compatibility is intentionally out of scope for now. Administrators with historical
  `.bin` databases can load them into stock PennMUSH and issue a text `@dump`, then ingest the result
  with PennMushSharp for parity development.

## Runtime Metadata Access
`PennMushSharp.Core.Metadata.MetadataCatalogs` exposes singleton access to the
flag/attribute/lock/function catalogs so upcoming subsystems (locks, commands, function
evaluation, persistence) can share the same canonical data. `LockMetadataService`,
`LockEvaluator`, `SimpleLockExpressionEngine`, and the new `InMemoryGameState`/`InMemoryLockStore`
pipeline show how metadata flows into runtime behaviour while we continue fleshing out full
persistence.

## Runtime Host & Commands
- `PennMushSharp.Runtime` composes the DI container via `RuntimeApplication` and boots through
  `DefaultServerBootstrapper`, which optionally loads `data/indb` (or `PennMushSharp:InitialDumpPath`)
  before exposing services like `CommandDispatcher`.
- The new command parser understands stacked commands (`;`/`&`), switch syntax (`/silent:room`), and
  divides input into structured `CommandInvocation`s. Metadata from `cmds.c` feeds `CommandCatalog`
  so canonical names and aliases point to the same handler.
- `LookCommand`/`WhoCommand` now consume the real dump data (room descriptions, WHO columns) and the
  dispatcher performs basic permission/switch validation via the metadata catalog ahead of command execution.
- Social commands are online: `SAY`, `POSE`, `SEMIPOSE`, `WHISPER`, `PAGE`, `@EMIT`, and `@PEMIT` mirror the
  legacy broadcast semantics, honor `Speech`/`Page` locks plus `HAVEN`, emit PennMUSH-style error strings, and
  use the session registry so room occupants receive output.
  The parser also recognizes classic shorthand tokens (`'`, `"`, `:`, `;`, `\`) so typing `'Hello` or `:waves`
  routes straight to the appropriate handler, just like the C server.
- `TelnetServer` (hosted background service) listens on `PennMushSharp:ListenAddress/ListenPort`
  (default `127.0.0.1:4201`). Clients log in via `CONNECT <name> [<password>]` (the stock dump ships
  with `One` and a blank password) or `CREATE <name> <password>`. Sessions record host/idle/command
  stats so WHO can mirror legacy output. Newly created accounts persist as PennMUSH-compatible dumps
  (default `PennMushSharp/data/accounts.dump`, override via `PennMushSharp:AccountStorePath`).

## Configuration & Logging
- `src/PennMushSharp.Runtime/appsettings.json` is copied into the runtime output and provides the default
  `PennMushSharp` option set (listen address/port, dump paths, seeded account) plus logging levels. Edit this
  file or drop an `appsettings.Development.json` next to it to override values locally. Environment variables
  such as `DOTNET_ENVIRONMENT=Development` or `PennMushSharp__ListenPort=4202` also flow through the generic
  host configuration pipeline.
- Logging is configured via `Microsoft.Extensions.Logging` using the `Logging` section in `appsettings.json`
  and emits structured console traces with timestamps and scopes so background services can be observed
  while the host runs indefinitely (Ctrl+C to stop).

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

## Coding Conventions
See `docs/coding-conventions.md` for formatting, naming, and test expectations. CI runs
`dotnet format` and the characterization checks, so ensure local changes follow those
guidelines before opening a pull request.
- Attribute system is read-only: no `&attr obj=value`, `@lock`, `@set`, or `$command` attribute handlers yet. Building out full attribute persistence and tying attribute triggers into the command processor (including MASTER_ROOM/global attr sourcing and zone inheritance) is the top priority.
- MUSHcode execution helpers such as `ufun()`, `eval()`, and `get()` are not implemented, so stored attribute programs cannot run in-line.
- Aliases extracted from `cmds.c`/`function.c` are not enforced—commands/functions currently only respond to their primary names.
- Movement/building are still stubbed: there are no `go`/exit commands, automatic exit alias binding, `@dig`, `@link`, etc.
- Zone mechanics, command/function inheritance, and event hooks (`@aconnect`, movement triggers, etc.) are not wired up yet.
- Parentage/inheritance (`@parent`, attribute lookup up the ancestry chain) is unimplemented, so builders cannot share commands/functions via parents; future enhancements may include an opt-in “extended inheritance” mode that purposely breaks dump compatibility.
- Communication subsystems beyond basic SAY/WHISPER/PAGE remain TODO (`@conformat`, the full chat service, bespoke mail persistence compatible with PennMUSH semantics).
