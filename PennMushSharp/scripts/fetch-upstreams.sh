#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

PENNMUSH_REPO=${PENNMUSH_REPO:-https://github.com/pennmush/pennmush}
PENNMUSH_REF=${PENNMUSH_REF:-master}
ASPACE_REPO=${ASPACE_REPO:-https://github.com/aspace-sim/aspace}
ASPACE_REF=${ASPACE_REF:-master}

usage() {
  cat <<USAGE
Usage: $0 [--pennmush-ref <ref>] [--aspace-ref <ref>]

Environment overrides:
  PENNMUSH_REPO (default: $PENNMUSH_REPO)
  PENNMUSH_REF  (default: $PENNMUSH_REF)
  ASPACE_REPO   (default: $ASPACE_REPO)
  ASPACE_REF    (default: $ASPACE_REF)
USAGE
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --pennmush-ref)
      PENNMUSH_REF=$2
      shift 2
      ;;
    --aspace-ref)
      ASPACE_REF=$2
      shift 2
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "Unknown option: $1" >&2
      usage
      exit 1
      ;;
  esac
done

sync_repo() {
  local name=$1 url=$2 ref=$3
  local dest="$ROOT_DIR/$name"

  if [[ -d "$dest/.git" ]]; then
    echo "Updating $name in $dest"
    git -C "$dest" fetch origin --tags
    git -C "$dest" checkout "$ref"
    git -C "$dest" pull --ff-only origin "$ref"
  else
    echo "Cloning $name ($ref)"
    git clone --branch "$ref" --depth 1 "$url" "$dest"
  fi
}

sync_repo "pennmush" "$PENNMUSH_REPO" "$PENNMUSH_REF"
sync_repo "aspace" "$ASPACE_REPO" "$ASPACE_REF"

echo "Dependencies are ready under $ROOT_DIR."
