#!/bin/bash
# Continuously ensure the simulator and F# watcher are running.

# Commands to identify processes
SIM_CMD="dotnet Pxl.Simulator"
FSI_PATTERN="dotnet fsi .*/apps_fsharp/fsxWatcher.fsx"

while true; do
  # Check simulator
  if ! pgrep -f "$SIM_CMD" > /dev/null; then
    echo "Simulator not running, starting..."
    ./build/start-simulator.sh &
  fi

  # Check F# watcher
  if ! pgrep -f "$FSI_PATTERN" > /dev/null; then
    echo "F# watcher not running, starting..."
    ./build/start-fsharp-watcher.sh &
  fi

  sleep 10
done
