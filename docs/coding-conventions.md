# Coding Conventions

This project follows the .editorconfig at the repo root and layers the
guidelines below so new code lands consistently.

## Languages & Formatting
- Target the current LTS (.NET 8 today) and prefer latest C# features when they
  keep parity code clear.
- All source files are UTF-8 with LF endings and no BOM. Keep files ASCII unless
  the upstream data requires otherwise.
- Indentation is two spaces. Use braces even for single-line statements; the CI
  `dotnet format` step will enforce this.
- Prefer file-scoped namespaces (`namespace Foo;`) and `var` when the type is
  obvious from the right-hand side. Keep explicit types for primitives, tuple
  literals, or when it improves readability.

## Project Layout
- Runtime components live under `src/` in projects that mirror the PennMUSH
  subsystems (`PennMushSharp.Core`, `.Runtime`, `.Commands`, `.Functions`,
  `.Space`). Tests live under `tests/` with matching folders.
- Tooling that parses upstream headers belongs in `tools/`, while legacy
  integration scripts go under `scripts/`.
- Documentation for subsystems lives in `docs/`; prefer short Markdown files and
  cross-link them from `README.md` when they affect day-to-day workflows.

## Naming & Structure
- Match PennMUSH terminology so parity discussions stay aligned. Use `DbRef`,
  `Lock`, `Attribute`, etc., instead of inventing new vocabulary.
- Keep public APIs immutable where possible (`record` types for value objects,
  readonly collections, etc.) to make characterization easier.
- Use dependency injection for runtime services; register them inside
  `RuntimeApplication.ConfigureServices`.

## Testing & Characterization
- Every extractor, parser, and runtime component should have unit tests in the
  matching `tests/` project. Prefer xUnit facts/theories and deterministic sample
  data checked into `tests/fixtures`.
- When behaviour depends on the legacy server, add or update a scenario under
  `tests/characterization/scenarios/`, capture a transcript via
  `scripts/characterize.sh`, and commit the golden log. `dotnet test` verifies
  the transcript metadata and scenario coverage.
- Before sending PRs, run `dotnet format` and `dotnet test PennMushSharp.sln`
  from the repo root; CI will fail if formatting or goldens drift.

## Source Control
- Keep commits logically scoped. Generated files under `docs/generated/` should
  only change when the upstream extractor runs.
- Do not rewrite the legacy goldens manually—always regenerate via the
  harness so they include the metadata banner expected by the tests.
