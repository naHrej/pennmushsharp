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
| 3. Command & Function Engine | Reimplement command parser, executor, permissions, + all builtin functions (string/math/logical/etc). | ✅ Parser supports stacking, switches, metadata-driven aliases, and permissions. 🚧 Next: full command table import, PennMUSH function evaluator, integration tests driving expressions. |
| 4. Game Systems | Port core behaviours (look, movement, building, mail, chat, queues, events). | Modules per subsystem with integration tests; minimal playable loop without spaceflight. |
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
- ✅ Solution + project scaffolding is in place with shared build props, editor config, and initial tests.
- ✅ Flag/power, attribute, lock, and builtin function extraction pipelines generate JSON snapshots from the C sources and are embedded via `FlagCatalog`, `AttributeCatalog`, `LockCatalog`, and `FunctionCatalog`.
- ✅ Metadata regeneration script (`scripts/regenerate-metadata.sh`) runs every extractor to keep the docs in sync.
- ✅ Text dump parser + in-memory state ingest modern PennMUSH dumps so lock metadata and attributes can flow directly into runtime services.
- ✅ Characterization harness drives the real PennMUSH binary via `scripts/start-legacy-mush.sh`, resetting the seeded database (`One` / _no password_) before each transcript capture so tests can diff deterministic output.
- ⏳ Runtime hardening (network/telnet loops, command implementations) and CI remain outstanding for Phase 0.

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
