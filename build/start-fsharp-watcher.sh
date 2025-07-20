#!/bin/bash
set -e

echo "Starting F# watcher ..."
dotnet fsi ./apps_fsharp/fsxWatcher.fsx
