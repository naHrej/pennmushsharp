#!/usr/bin/env bash
set -euo pipefail
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
export DOTNET_CLI_HOME="${ROOT_DIR}/.dotnet-cli"
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_NOLOGO=1
pushd "${ROOT_DIR}" >/dev/null
 dotnet run --project tools/FlagTableExtractor/FlagTableExtractor.csproj --configuration Release
 dotnet run --project tools/AttributeTableExtractor/AttributeTableExtractor.csproj --configuration Release
 dotnet run --project tools/LockTableExtractor/LockTableExtractor.csproj --configuration Release
 dotnet run --project tools/FunctionTableExtractor/FunctionTableExtractor.csproj --configuration Release
 dotnet run --project tools/CommandTableExtractor/CommandTableExtractor.csproj --configuration Release
popd >/dev/null
