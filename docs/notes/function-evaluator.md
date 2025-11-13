# Function Evaluator Design Notes

## Register Semantics
- `%0-%9` bind to command arguments; `%q0-%9`/`%qa-%qz` are scratch pads set via `setq()`. These originated as fixed arrays because the legacy parser split its args into static slots. PennMushSharp isn’t bound by those constraints, so the evaluator will:
  * Expose every argument supplied to a command, not just the first ten. If a user passes 12 args, `%10`, `%11`, etc. will resolve instead of truncating at `%9`.
  * Extend `%q` registers into a string-keyed map. `setq(foo,bar)` stores into `%qfoo`, and the legacy single-character registers continue to work for compatibility. `setq/r` both accept arbitrary identifiers, so macro authors can name their scratch variables instead of counting slots.

- Rationale: the original limits came from PennMUSH’s C architecture (fixed arrays in `PE_REGS`, hard-coded stack frames). Our managed evaluator can allocate maps on demand, so we retain compatibility (the classic names still exist) while removing ceilings that only existed due to legacy constraints.
