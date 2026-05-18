#!/bin/bash

# Configuration
API_PORT=5249
WEB_PORT=4200
FIREBASE_PORT=9099
PROJECT_ROOT=$(pwd)

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

# Cleanup function for E2E
cleanup_e2e() {
  echo -e "\nStopping E2E environment..."
  kill $API_PID $WEB_PID 2>/dev/null
  stop_port $API_PORT
  stop_port $WEB_PORT
  stop_firebase
}

# Unit Tests
run_unit() {
  echo "Running Unit Tests..."
  echo "--- Frontend Unit Tests ---"
  cmd //c "npm.cmd test --prefix src/Micro.Web -- --run"
}

# Integration Tests
run_integration() {
  local test_filter=$1
  echo "Running Integration Tests..."
  start_firebase
  if [ ! -z "$test_filter" ]; then
    dotnet test tests/Micro.Tests/Micro.Tests.csproj --filter "$test_filter"
  else
    dotnet test tests/Micro.Tests/Micro.Tests.csproj
  fi
  local exit_code=$?
  stop_firebase
  return $exit_code
}

# E2E Tests
run_e2e() {
  local test_filter=$1
  echo "Preparing E2E Environment..."
  stop_port $API_PORT
  stop_port $WEB_PORT
  
  start_firebase

  echo "Starting Services..."
  # Start Backend
  export ASPNETCORE_ENVIRONMENT=Testing
  dotnet run --project src/Micro.API --urls=http://localhost:$API_PORT --no-launch-profile --environment Testing &
  API_PID=$!

  # Start Frontend (Dev server handles proxying)
  cmd //c "npm.cmd start --prefix src/Micro.Web -- --port $WEB_PORT" &
  WEB_PID=$!

  # Parallel Wait
  wait_for_url "http://localhost:$API_PORT/health" "API" 45 || { cleanup_e2e; return 1; }
  wait_for_url "http://localhost:$WEB_PORT" "Web" 60 || { cleanup_e2e; return 1; }

  echo "Running E2E tests..."
  if [ -n "$test_filter" ]; then
    echo "Filter: $test_filter"
    dotnet test tests/Micro.E2E/Micro.E2E.csproj --filter "FullyQualifiedName~$test_filter"
  else
    dotnet test tests/Micro.E2E/Micro.E2E.csproj
  fi
  local exit_code=$?

  cleanup_e2e
  return $exit_code
}

# Main Logic
MODE=$1
FILTER=$2

case $MODE in
  "unit")
    run_unit
    ;;
  "integration")
    run_integration "$FILTER"
    ;;
  "e2e")
    run_e2e "$FILTER"
    ;;
  "all")
    run_unit
    run_integration
    run_e2e
    ;;
  *)
    echo "Usage: ./scripts/test.sh {unit|integration|e2e|all}"
    exit 1
    ;;
esac
