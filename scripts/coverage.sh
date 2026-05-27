#!/bin/bash

# Configuration
FIREBASE_PORT=9099

# Helper function to stop a process by port
stop_port() {
  local port=$1
  if [[ "$OSTYPE" == "msys" || "$OSTYPE" == "win32" ]]; then
    local pid=$(netstat -ano | grep LISTENING | grep ":$port " | awk '{print $5}' | head -n 1)
    if [ ! -z "$pid" ]; then
      echo "Stopping process on port $port (PID: $pid)..."
      taskkill //F //PID $pid 2>/dev/null
    fi
  else
    local pid=$(lsof -t -i:$port)
    if [ ! -z "$pid" ]; then
      echo "Stopping process on port $port (PID: $pid)..."
      kill -9 $pid 2>/dev/null
    fi
  fi
}

# Optimized wait function
wait_for_url() {
  local url=$1
  local name=$2
  local timeout=${3:-60}
  echo "Waiting for $name at $url..."
  for i in $(seq 1 $timeout); do
    if curl -s "$url" > /dev/null; then
      echo "$name is ready."
      return 0
    fi
    sleep 1
  done
  echo "$name timed out."
  return 1
}

# Firebase Emulator
start_firebase() {
  echo "Starting Firebase Emulator..."
  stop_port $FIREBASE_PORT
  firebase emulators:start --only auth --project demo-micro-ats &
  FIREBASE_PID=$!
  wait_for_url "http://localhost:$FIREBASE_PORT" "Firebase" 15
}

stop_firebase() {
  echo "Stopping Firebase Emulator..."
  kill $FIREBASE_PID 2>/dev/null
  stop_port $FIREBASE_PORT
}

# Run tests with coverage for the Integration/Unit tests only
# Exclude system, Microsoft namespaces and any generated files
start_firebase
dotnet test tests/Micro.Tests/Micro.Tests.csproj --collect:"XPlat Code Coverage" -- \
  DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Exclude="[System.*]*,[Microsoft.*]*" \
  DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.ExcludeByFile="**/*.generated.cs"
local_exit_code=$?
stop_firebase

# Generate report
# Filter out system assemblies and generated files to avoid "file not exist" errors
reportgenerator \
-reports:"tests/Micro.Tests/TestResults/*/coverage.cobertura.xml" \
-targetdir:"tests/Micro.Tests/TestResults/CoverageReport" \
-reporttypes:Html \
-assemblyfilters:"-System*;-Microsoft*" \
-filefilters:"-*.generated.cs"

echo "Coverage report generated at: tests/Micro.Tests/TestResults/CoverageReport/index.html"
exit $local_exit_code
