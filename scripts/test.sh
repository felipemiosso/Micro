#!/bin/bash

# Configuration
API_PORT=5249
WEB_PORT=4200
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

# Cleanup function for E2E
cleanup_e2e() {
  echo -e "\nStopping E2E environment..."
  kill $API_PID $WEB_PID 2>/dev/null
  stop_port $API_PORT
  stop_port $WEB_PORT
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
  echo "Running Integration Tests..."
  dotnet test tests/Micro.Tests/Micro.Tests.csproj
}

# E2E Tests (Playwright)
run_e2e() {
  echo "Preparing E2E Environment..."
  stop_port $API_PORT
  stop_port $WEB_PORT

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
  dotnet test tests/Micro.E2E/Micro.E2E.csproj
  local exit_code=$?

  cleanup_e2e
  return $exit_code
}

# Main Logic
MODE=$1

case $MODE in
  "unit")
    run_unit
    ;;
  "integration")
    run_integration
    ;;
  "e2e")
    run_e2e
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
