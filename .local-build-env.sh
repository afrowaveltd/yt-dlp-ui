#!/usr/bin/env bash
# Source this before building on machines where ~/.dotnet and ~/.nuget are not writable.
# Usage:  source ./.local-build-env.sh && dotnet build YtDlpUi.sln
export DOTNET_CLI_HOME="$PWD/.dotnet-home"
export NUGET_PACKAGES="$PWD/.nuget-packages"
export DOTNET_NOLOGO=1
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
mkdir -p "$DOTNET_CLI_HOME" "$NUGET_PACKAGES"
