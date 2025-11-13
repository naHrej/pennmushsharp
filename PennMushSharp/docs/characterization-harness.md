# Characterization Harness Plan

1. Launch legacy PennMUSH via `game/mush` (compiled from `pennmush/`).
2. Drive scripted command sets via `scripts/characterize.sh` to capture stdout/stderr and saved db dumps.
3. Store golden transcripts in `tests/characterization/output` (or a scenario-specific folder) and check them into git alongside the scenario file.
4. Mirror each transcript with a .NET integration test to ensure the port matches byte-for-byte output.

## Scenario Format

Scenario files live under `tests/characterization/scenarios/` and are simple bash scripts
that set a few variables consumed by the harness. Example (`look_basic.scenario`):

```bash
NAME="look_basic"
HOST="127.0.0.1"
PORT=4201
USER="One"
PASSWORD=""
COMMANDS=(
  "look"
  "who"
)
DISCONNECT_COMMAND="QUIT"
# Optional: let the harness spawn the legacy server
#SERVER_EXEC="../../pennmush/game/netmush"
#SERVER_ARGS=("--no-session" "mush.cnf")
#SERVER_WORKDIR="../../pennmush/game"
```

- `COMMANDS` is a bash array of commands to run after the harness issues a
  `connect USER PASSWORD` line.
- If `SERVER_EXEC` is provided, the harness will start the binary and wait for
  the configured port before running commands.

## Running the Harness

```bash
scripts/characterize.sh --scenario tests/characterization/scenarios/look_basic.scenario \
  --output tests/characterization/output
```

This command will:

1. Ensure `nc` (netcat) is available.
2. Start the server if `SERVER_EXEC` is provided (otherwise it assumes the game is already running).
3. Connect to `HOST:PORT`, log in with the provided credentials, run each command with a short delay, and optionally execute `DISCONNECT_COMMAND`.
4. Save the transcript to `tests/characterization/output/<scenario>-<timestamp>.log` and echo the path.

Once transcripts look correct, copy them into a stable location
(e.g., `tests/characterization/golden/<scenario>.log`) and wire an integration test that diffs the new C# behaviour against that file.

## Bootstrapping the Legacy Server

- Use `scripts/start-legacy-mush.sh` as `SERVER_EXEC` to spin up the freshly built PennMUSH tree in
  `../pennmush/game`. The script copies `game/data/indb.seed.gz` into `indb.gz` before every launch
  so you start from an identical world snapshot each run.
- The seeded database contains the wizard account `One` with no password. Scenarios
  can authenticate with those credentials (as `look_basic.scenario` does) to issue commands such as
  `look`/`who` during transcript capture.

## Managing Golden Transcripts

- Pass `--golden` to `scripts/characterize.sh` to automatically copy the freshly
  captured transcript into `tests/characterization/golden/<scenario>.log`.
- Override the destination with `--golden-dir <path>` if you want to stage
  golden logs elsewhere before committing them.
- Each scenario should keep one canonical golden log so the .NET tests can
  validate behaviour with `dotnet test`.
