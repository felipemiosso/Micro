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
      taskkill //F //PID $pid
    fi
  else
    local pid=$(lsof -t -i:$port)
    if [ ! -z "$pid" ]; then
      echo "Stopping process on port $port (PID: $pid)..."
      kill -9 $pid
    fi
  fi
}

# Firebase Emulator
start_firebase() {
  echo "Starting Firebase Emulator..."
  stop_port $FIREBASE_PORT
  firebase emulators:start --only auth --project demo-micro-ats &
  FIREBASE_PID=$!
  sleep 5
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

# Unit Tests (Angular + .NET if any)
run_unit() {
  echo "Running Unit Tests..."
  # Frontend Unit Tests (using Vitest as seen in package.json)
  echo "--- Frontend Unit Tests ---"
  cmd //c "npm.cmd test --prefix src/Micro.Web -- --run"
  
  # Backend Unit Tests (Currently none separated from integration, but placeholder)
  echo "--- Backend Unit Tests ---"
  echo "No dedicated backend unit test project found."
}

# Integration Tests (.NET Endpoints)
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

# E2E Tests (Playwright)
run_e2e() {
  local test_filter=$1
  echo "Preparing E2E Environment..."
  stop_port $API_PORT
  stop_port $WEB_PORT
  start_firebase

  echo "Starting Backend (Micro.API)..."
  export ASPNETCORE_ENVIRONMENT=Testing
  dotnet run --project src/Micro.API --urls=http://localhost:$API_PORT --no-launch-profile --environment Testing &
  API_PID=$!

  echo "Starting Frontend (Micro.Web)..."
  cmd //c "npm.cmd start --prefix src/Micro.Web -- --port $WEB_PORT" &
  WEB_PID=$!

  echo "Waiting for Backend (Health Check)..."
  sleep 10
  for i in {1..30}; do
    if curl -s http://localhost:$API_PORT/health > /dev/null; then
      echo "Backend is alive."
      break
    fi
    echo "Retrying backend check ($i/30)..."
    sleep 2
  done

  echo "Waiting for Frontend..."
  sleep 15
  for i in {1..30}; do
    if curl -s http://localhost:$WEB_PORT > /dev/null; then
      echo "Frontend is ready."
      break
    fi
    echo "Retrying frontend check ($i/30)..."
    sleep 2
  done

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
