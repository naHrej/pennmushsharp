#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR=$(cd -- "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd)
if [[ -n "${PENNMUSH_ROOT:-}" ]]; then
  GAME_DIR="$PENNMUSH_ROOT/game"
else
  GAME_DIR=$(realpath "$SCRIPT_DIR/../../pennmush/game")
fi
PENNMUSH_ROOT=$(dirname "$GAME_DIR")
DATA_DIR="$GAME_DIR/data"
SEED_DB=${PENNMUSH_SEED:-"$DATA_DIR/indb.seed.gz"}

reset_db() {
  if [[ -f "$SEED_DB" ]]; then
    cp "$SEED_DB" "$DATA_DIR/indb.gz"
    gunzip -c "$DATA_DIR/indb.gz" > "$DATA_DIR/indb"
  fi

  for name in maildb chatdb; do
    if [[ ! -f "$DATA_DIR/${name}.gz" ]]; then
      : > "$DATA_DIR/$name"
      gzip -c "$DATA_DIR/$name" > "$DATA_DIR/${name}.gz"
    fi
  done
}

reset_db

cd "$GAME_DIR"
exec ./netmush "$@"
