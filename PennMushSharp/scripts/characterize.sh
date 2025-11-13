#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<USAGE
Usage: $0 --scenario <file> [--output <dir>] [--command-delay <seconds>] [--golden]

Options:
  -s, --scenario       Path to a scenario file (bash syntax).
  -o, --output         Directory for transcripts (default: tests/characterization/output).
  -d, --command-delay  Seconds to wait between commands (default: 1).
  -g, --golden         Also copy the transcript into the golden directory for the scenario.
      --golden-dir     Override the default golden directory (tests/characterization/golden).
USAGE
}

SCENARIO=""
OUTPUT_DIR="tests/characterization/output"
COMMAND_DELAY=1
UPDATE_GOLDEN=false
GOLDEN_DIR="tests/characterization/golden"

while [[ $# -gt 0 ]]; do
  case "$1" in
    -s|--scenario)
      SCENARIO=$2
      shift 2
      ;;
    -o|--output)
      OUTPUT_DIR=$2
      shift 2
      ;;
    -d|--command-delay)
      COMMAND_DELAY=$2
      shift 2
      ;;
    -g|--golden)
      UPDATE_GOLDEN=true
      shift
      ;;
    --golden-dir)
      GOLDEN_DIR=$2
      shift 2
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "Unknown argument: $1" >&2
      usage
      exit 1
      ;;
  esac
done

if ! command -v nc >/dev/null 2>&1; then
  echo "nc (netcat) is required to run the characterization harness." >&2
  exit 1
fi

if [[ -z "$SCENARIO" ]]; then
  echo "Scenario file is required." >&2
  usage
  exit 1
fi

if [[ ! -f "$SCENARIO" ]]; then
  echo "Scenario file '$SCENARIO' not found." >&2
  exit 1
fi

# shellcheck source=/dev/null
source "$SCENARIO"

: "${NAME:?Scenario must define NAME}" 
: "${HOST:=127.0.0.1}" 
: "${PORT:?Scenario must define PORT}" 
: "${USER:?Scenario must define USER}" 
: "${PASSWORD:=}" 
if ! declare -p COMMANDS >/dev/null 2>&1; then
  echo "Scenario must define COMMANDS array." >&2
  exit 1
fi

mkdir -p "$OUTPUT_DIR"
TRANSCRIPT="$OUTPUT_DIR/${NAME}-$(date -u +%Y%m%dT%H%M%SZ).log"
if [[ "$UPDATE_GOLDEN" == true ]]; then
  mkdir -p "$GOLDEN_DIR"
fi
SERVER_PID=""

cleanup() {
  if [[ -n "$SERVER_PID" ]]; then
    kill "$SERVER_PID" >/dev/null 2>&1 || true
  fi
}

wait_for_port() {
  local retries=30
  while (( retries > 0 )); do
    if nc -z "$HOST" "$PORT" >/dev/null 2>&1; then
      return 0
    fi
    sleep 1
    ((retries--))
  done
  echo "Timed out waiting for $HOST:$PORT" >&2
  exit 1
}

start_server() {
  local exec_path="$SERVER_EXEC"
  local workdir="${SERVER_WORKDIR:-$(dirname "$exec_path")}" 
  pushd "$workdir" >/dev/null
  if declare -p SERVER_ARGS >/dev/null 2>&1; then
    "${exec_path}" "${SERVER_ARGS[@]}" >/dev/null 2>&1 &
  else
    "${exec_path}" >/dev/null 2>&1 &
  fi
  SERVER_PID=$!
  popd >/dev/null
  trap cleanup EXIT INT TERM
}

if [[ -n "${SERVER_EXEC:-}" ]]; then
  start_server
fi

wait_for_port

{
  sleep "$COMMAND_DELAY"
  printf 'connect %s %s\r\n' "$USER" "$PASSWORD"
  for cmd in "${COMMANDS[@]}"; do
    sleep "$COMMAND_DELAY"
    printf '%s\r\n' "$cmd"
  done
  if [[ -n "${DISCONNECT_COMMAND:-}" ]]; then
    sleep "$COMMAND_DELAY"
    printf '%s\r\n' "$DISCONNECT_COMMAND"
  fi
  sleep "$COMMAND_DELAY"
} | nc "$HOST" "$PORT" | tee "$TRANSCRIPT"

echo "Transcript saved to $TRANSCRIPT"

if [[ "$UPDATE_GOLDEN" == true ]]; then
  GOLDEN_PATH="$GOLDEN_DIR/${NAME}.log"
  {
    printf '*** PennMUSH Legacy :: %s ***\n' "$NAME"
    cat "$TRANSCRIPT"
  } > "$GOLDEN_PATH"
  echo "Golden transcript updated at $GOLDEN_PATH"
fi
