# PennMushSharp Roadmap

## Vision
Recreate the classic PennMUSH server plus the Aspace spaceflight engine as a modern, maintainable .NET solution while preserving gameplay semantics and data compatibility. The port should feel native to C#, emphasize testability, and make it easy to extend the codebase with new commands, functions, and simulation rules.

## Guiding Principles
- **Parity first:** Match observable behaviour (commands, functions, database formats) before adding new features.
- **Iterative porting:** Replace subsystems one by one with C# equivalents while leaning on characterization tests to lock down behaviour.
- **Modularization:** Separate core MUSH engine, shared utilities, and spaceflight logic into focused projects to keep responsibilities clear.
- **Automation:** Introduce comprehensive tests and tooling early so regressions are caught while the codebase is small.

## Major Workstreams & Milestones
| Phase | Goal | Key Deliverables |
| --- | --- | --- |
| 0. Discovery & Tooling | Capture requirements, choose .NET stack, set up CI/testing harness. | Repo layout (`PennMushSharp.sln`), coding conventions, baseline CI workflow, characterization test harness against existing C server. |
| 1. Core Runtime Skeleton | Stand up the process host, configuration system, logging, dependency injection. | `PennMushSharp.Runtime` project, config loader, structured logging, basic service container. |
| 2. Database & Object Model | Port dbref/object schema, attributes, flags, lock parsing, persistence adapters. | `PennMushSharp.Core` with object/attribute data types, text DB loaders/serializers, serialization tests ensuring compatibility with PennMUSH text dumps. (Binary dumps are intentionally out of scope; convert via stock PennMUSH + `@dump` when needed.) |
| 3. Command & Function Engine | Reimplement command parser, executor, permissions, + all builtin functions (string/math/logical/etc). | ✅ Parser supports stacking, switches, metadata-driven aliases, and permissions. 🚧 Next: load full `command.c` metadata, implement the function evaluator (with modernized `%q`/`%0+` semantics: string-keyed `%qfoo` and unlimited `%10+` args), and build expression tests. |
| 4. Game Systems | Port core behaviours (look, movement, building, mail, chat, queues, events). | Modules per subsystem with integration tests; minimal playable loop without spaceflight. Includes configurable HTML5/CSS output templates so commands can emit both classic text and rich markup for modern clients, plus a modern scheduler/task queue that lifts legacy limits while honoring PennMUSH throttles. |
| 5. Networking & Client IO | Implement telnet/WebSocket adapters, connection management, ANSI pipeline. | Network gateway project, session/auth handling, compatibility tests with common MU* clients. |
| 6. Spaceflight Integration | Bring over Aspace simulation: data structures, physics loop, command set, UI. | `PennMushSharp.Space` project, tick scheduler, command bindings, telemetry/logging. |
| 7. Tooling & Extensibility | Scripting hooks, plugin surface, admin tooling. | Plugin SDK, scripting bridge (potentially Roslyn or Lua), admin CLI/web panel stubs. |
| 8. Stabilization & Release | Performance tuning, docs, packaging. | Load/perf benchmarks, migration guides, docker images, release checklist. |

## Canonical Feature Inventory (PennMUSH + Aspace)
| Area | Representative C Sources | Notes for C# Parity |
| --- | --- | --- |
| Boot/runtime, config, logging | `game.c`, `conf.c`, `options.h`, `log.c`, `mysocket.c`, `bsd.c`, `ssl_master.c`, `ssl_slave.c`, `websock.c`, `mysocket.c`, `myrlimit.c`, `myssl.c`, `sig.c`, `timer.c` | Process lifecycle, signal handling, telnet/WebSocket servers, TLS, SSL proxies, rate limits, scheduling. |
| Persistence & schema | `db.c`, `dbtools/`, `create.c`, `destroy.c`, `attrib.c`, `atr_tab.c`, `set.c`, `flags.c`, `bflags.c`, `boolexp.c`, `lock.c`, `hash_function.c`, `ptab.c`, `strtree.c`, `compress.c`, `sql.c`, `sqlite3.c`, `uint.c` | Database formats (compressed text/binary), object graph, attribute/flag tables, locks/boolexps, SQL bridge, UTF-8/string utilities. |
| Command/permission system | `command.c`, `cmds.c`, `cmdlocal.dst`, `match.c`, `access.c`, `privtab.c`, `wiz.c`, `rob.c`, `malias.c`, `speech.c`, `notify.c`, `look.c`, `move.c`, `wait.c`, `queue.c`, `cque.c` | Parser, dispatcher, privilege tables, builtin command modules (building, mail, social, economy, queue). |
| Function engine | `function.c`, `fundb.c`, `funstr.c`, `funmath.c`, `funcrypt.c`, `funlist.c`, `funmisc.c`, `funjson.c`, `funufun.c`, `lmathtab.c`, `htmltab.c`, `jsontypes.c`, `tables.c`, `funlocal.dst` | String/math/logical/calc libs, JSON helpers, cJSON integration, localization tables. |
| Communication & services | `extchat.c`, `extmail.c`, `connlog.c`, `portmsg.c`, `services.c`, `extmail.c`, `mail.c` (in `cmds.c`), `help.c`, `markup.c`, `htmltab.c`, `spellfix.c`, `notify.c` | External chat, mail relays, connection logging, port messaging, markup/ANSI helpers, help system. |
| Eventing & utilities | `bufferq.c`, `chunk.c`, `mysocket.c`, `wait.c`, `timer.c`, `map_file.c`, `utils.c`, `memcheck.c`, `myrlimit.c`, `pcg_basic.c`, `intmap.c` | Buffer queues, chunk allocators, timing, RNG, memory tracking, map file reader. |
| Internationalization & docs | `po/`, `htmldocs/`, `I18N.md`, `FAQ.md`, `UPGRADING.md` | Need doc converters and i18n resource handling. |
| Spaceflight (Aspace) | `space_main.c` plus `space_*.c` modules mirrored in `pennmush/src` and `aspace/src` | Data model (vessels, queues, nebulae), physics tick, messaging, configuration, commands. |

The goal is to tick each row off only when its underlying behaviour, configuration, admin tooling, and data interchange are reproduced or superseded with verified tests.

## Immediate Next Steps
1. Wire the runtime host’s dependency injection container (metadata catalogs, lock services, persistence abstractions) so future subsystems can plug into a real process host instead of no-op stubs.
2. Promote the new persistence pipeline from “parser + in-memory state” to a reusable adapter that can hydrate live game objects from stock dumps or future storage engines.
3. Continue building automated parity signals from checked-in dumps and golden transcripts while we explore alternatives to running the legacy C server locally.
4. Lay down CI (GitHub Actions) that runs `dotnet format`, `dotnet build`, `dotnet test`, and verifies the generated artifacts are up to date.

## Phase 0 Task Breakdown
1. **Solution Scaffolding**
   - Create `PennMushSharp.sln`.
   - Projects: `PennMushSharp.Runtime` (host), `PennMushSharp.Core` (domain + persistence), `PennMushSharp.Commands`, `PennMushSharp.Functions`, `PennMushSharp.Space`, and `PennMushSharp.Tests`.
   - Wire shared props/Directory.Build.props for analyzers, nullable, `implicit usings`.
2. **Characterization Harness**
   - Scripted launcher (likely `scripts/characterize.sh`) spins up the C server, drives commands, dumps canonical outputs for regression checks.
   - Store fixtures under `tests/characterization/`.
3. **Domain Documentation**
   - Extract flag/attribute tables from `pennmush/src/*.dst` into markdown specs.
   - Document DB formats (flat text, GZ, SQL) and migration strategy.
4. **Engineering Standards**
   - Add `.editorconfig`, `.gitignore`, CODEOWNERS, and CI workflow that runs `dotnet build`, `dotnet test`, formatting, and linting.
   - Define logging/eventing conventions (structured logging, tracing IDs) for parity with `log.c`.

Task tracking should reference this roadmap section to ensure the 1:1 parity requirement remains visible for every deliverable.

## Current Status (Phase 0)
- ✅ **Tooling & Standards:** Repo scaffolding, shared `Directory.Build.props`, `.editorconfig`, and linting/test workflows are live in CI. `scripts/fetch-upstreams.sh`/`regenerate-metadata.sh` keep external sources and generated docs synchronized.
- ✅ **Metadata Extraction:** Flag, attribute, lock, function, and command table extractors emit `docs/generated/*.json` snapshots that are embedded by the runtime catalogs to guarantee perfect parity with upstream headers.
- ✅ **Documentation:** README/roadmap/domain-model docs spell out compatibility guarantees, modernization goals (HTML5 output, extended registers/args), and onboarding instructions for the harness and runtime host.
- ✅ **Characterization Harness:** `scripts/start-legacy-mush.sh`+`scripts/characterize.sh` boot the C server, reset the seeded DB (`One` / no password), capture golden transcripts, and enforce coverage through `CharacterizationGoldenTests`.
- ✅ **Dump Compatibility:** `TextDumpParser`, `InMemoryGameState`, and lock services ingest stock PennMUSH text dumps—including the checked-in `data/indb`—so we can develop against real data without running netmush locally.
- ✅ **Runtime Skeleton:** The telnet host, session registry, command dispatcher (`LOOK`, `WHO`, `@EVAL`), and metadata-driven parser are wired through DI, giving us a stable execution harness for later phases.

## Current Status (Phase 1 – Core Runtime Skeleton)
- ✅ **Host & Configuration:** `RuntimeApplication` builds a `HostApplicationBuilder` pipeline with layered config sources, environment overrides, and `appsettings.json` support. Launch profiles + `launch.json` ensure F5 launches the telnet server with the correct working directory.
- ✅ **Logging & Diagnostics:** Structured console logging with timestamped scopes is standardized across runtime services; telnet/timer services log bootstrap and shutdown events for harness visibility.
- ✅ **Dependency Injection Surface:** All foundational services (metadata catalogs, lock services, persistence adapters, session registry, command dispatcher) are registered through Microsoft.Extensions.DependencyInjection, enabling unit tests to compose real hosts.
- ✅ **Session & Auth Loop:** `TelnetServer` hosts CONNECT/CREATE flows, password verification, session tracking, and command dispatching, so the managed runtime already supports a full login + command loop off the stock dump.
- ⚙️ **Next for Phase 1:** Harden graceful shutdown (e.g., CTS fan-out), add health probes, and expose structured metrics hooks so later phases (network multiplexers, queues) can observe the runtime host.

## Current Status (Phase 2 – Database & Object Model)
- ✅ **Schema Parity:** `GameObjectRecord`/`GameObject` mirror PennMUSH dbrefs, owners, flags, attributes, locks, and list/contents pointers so command modules can navigate the exact same topology as the C engine.
- ✅ **Text Dump IO:** `TextDumpParser` + `TextDumpWriter` handle the modern text dump syntax, including attribute blocks, locks, compression markers, and metadata banners. The runtime boots from the checked-in `data/indb` and any stock dump dropped into `PennMushSharp/data/`.
- ✅ **Lock & Attribute Catalogs:** Extracted metadata feeds `LockEvaluator`, `LockMetadataService`, and `InMemoryLockStore`, allowing live lock evaluation immediately after ingesting a dump.
- ✅ **Account/Persistence Bridges:** `TextDumpAccountRepository`, `AccountService`, and `PasswordVerifier` operate directly on dump data so telnet auth can create/connect characters without bespoke storage.
- ✅ **Modernization Decisions:** Binary dump compatibility is intentionally deferred (documented in README/ROADMAP), with guidance to round-trip through stock PennMUSH for conversions.
- ⚙️ **Next for Phase 2:** Implement pluggable persistence backends (e.g., JSON/SQL providers), differential dumps, and validation tooling to compare in-memory state against golden dumps as part of CI.

## Current Status (Phase 3 – Command & Function Engine)
- ✅ **Parser & Metadata:** `CommandParser` splits stacked commands (`;`, `&`), switch/argument tokens, and produces structured `CommandInvocation`s. Extracted `command.c` metadata drives aliases, switch declarations, wizard gating, and type flags so dispatch mirrors PennMUSH tables.
- ✅ **Dispatcher Pipeline:** `CommandDispatcher` validates switches/permissions, evaluates expressions unless `/NOEVAL`/`CMD_T_NOPARSE` apply, and feeds commands real metadata context, matching legacy parsing behaviour.
- ✅ **Command Coverage (Initial):** `LOOK`, `WHO`, and `@EVAL` run against real dump data, respect PennMUSH semantics (no self in contents, WHO columns/idle calculations), and are exercised via golden transcript tests.
- ✅ **Expression & Function Evaluators:** Nested `[]` expressions, `%q` registers (now unlimited and string-keyed), and `%0+` arguments resolve through the new evaluator stack. The function registry loads metadata from `functions.json`, enforces PennMUSH arity rules, honors aliases, and currently ships with register helpers, extended math (`ADD`/`SUB`/`MUL`/`DIV`/`MOD`/`ABS`/`MIN`/`MAX`/`CEIL`/`FLOOR`/`ROUND`/`ROOT`/`RAND`), trig/log/angle helpers (`SIN`/`COS`/`TAN`/`ASIN`/`ACOS`/`ATAN`/`ATAN2`/`CTU`/`LOG`/`LN`/`PI`/`POWER`), and string helpers (`UPCASE`/`DOWNCASE`/`STRLEN`/`TRIM`/`LTRIM`/`RTRIM`/`LEFT`/`RIGHT`/`MID`/`REPEAT`).
- ⚙️ **In Progress:** Import the remainder of the builtin command metadata, expand the function registry (string/math/list/JSON families), wire permissions/queues, and add characterization fixtures (LOOK/WHO/… scenarios) to drive behavioural parity tests for each command family.

## Progress Log
| Date (UTC) | Milestone | Notes |
| --- | --- | --- |
| 2025-11-11 | Repository scaffolded | `.NET 8` solution + shared props/projects created; roadmap drafted. |
| 2025-11-11 | Flag extractor & runtime catalog | `FlagTableExtractor`, `docs/generated/flags.json`, and `FlagCatalog` with unit coverage landed. |
| 2025-11-11 | Attribute extractor & catalog | Added attribute pipeline + docs and wired `AttributeCatalog` with tests. |
| 2025-11-11 | Lock/function extractors & catalogs | Introduced shared extraction library, lock/function pipelines, metadata regeneration script, and runtime catalogs with unit coverage. |
| 2025-11-11 | Metadata access layer | Added `MetadataCatalogs`, `LockMetadataService`, and unit coverage to begin consuming the generated data inside runtime services. |
| 2025-11-11 | Lock evaluator scaffold | Built the initial lock evaluation pipeline (`LockEvaluator`, expression/store interfaces, unit tests) to start wiring metadata into permission checks. |
| 2025-11-11 | Simple lock expression engine | Added a recursive-descent evaluator that handles boolean operators, `#dbref` literals, and default expressions so metadata-driven locks can execute immediately while we design full persistence. |
| 2025-11-11 | In-memory game state | Added `InMemoryGameState` and `InMemoryLockStore` so lock data loaded from persistence can register live lock expressions feeding the evaluator. |
| 2025-11-11 | Characterization harness tooling | Implemented `scripts/characterize.sh`, documented scenario format, and added the first sample scenario for capturing transcripts from the legacy server. |
| 2025-11-12 | Characterization scaffolding | Added golden transcript workflow, a sample transcript, and regression tests so every scenario stays paired with an updated capture. |
| 2025-11-12 | Text dump ingestion | `TextDumpParser` now understands the modern dump format and populates `InMemoryGameState`, enabling runtime services to consume real objects without running the legacy binary. |
| 2025-11-12 | Legacy harness automation | Added `scripts/start-legacy-mush.sh`, seeded credentials, and recorded the first golden transcript so regression tests can exercise the actual C server. |
| 2025-11-12 | Telnet runtime scaffold | Introduced the hosted telnet server, command dispatcher upgrades (`LOOK`, `WHO`), and session registry + tests so the managed runtime exposes a minimal interactive loop. |
| 2025-11-12 | Runtime DI + command scaffold | The .NET host composes metadata, persistence, and the new `CommandDispatcher`/`LookCommand`, providing a foundation for future telnet/command subsystems. |
| 2025-11-12 | Telnet auth commands | Added CONNECT/CREATE flows, password verification, and session tracking so the managed runtime mirrors the legacy login experience. |

## Tracking Progress
- Each phase should culminate in a short demoable milestone (e.g., "can connect and run `look`" or "spaceflight tick updates position").
- Use GitHub issues/projects to break phases into epics/stories; link commits/PRs back to roadmap items.
- Keep this roadmap living—update milestones, add risks, and log decisions as the port evolves.
