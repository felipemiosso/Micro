#!/bin/bash

# Configuration
API_PORT=5249
WEB_PORT=4200
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
  local timeout=${3:-30}
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

cleanup() {
  echo -e "\nStopping environment..."
  kill $API_PID $WEB_PID $FIREBASE_PID 2>/dev/null
  stop_port $API_PORT
  stop_port $WEB_PORT
  stop_port $FIREBASE_PORT
  exit
}

trap cleanup SIGINT SIGTERM

# Cleanup before start
stop_port $API_PORT
stop_port $WEB_PORT
stop_port $FIREBASE_PORT

echo "Starting Firebase Emulator..."
firebase emulators:start --only auth --project demo-micro-ats &
FIREBASE_PID=$!

echo "Starting Backend (Micro.API)..."
dotnet run --project src/Micro.API --urls=http://localhost:$API_PORT &
API_PID=$!

# Wait for API before starting Frontend to reduce resource contention
wait_for_url "http://localhost:$API_PORT/health" "API" 30

echo "Starting Frontend (Micro.Web)..."
cmd //c "npm.cmd start --prefix src/Micro.Web -- --port $WEB_PORT" &
WEB_PID=$!

echo "Dev environment running. Press Ctrl+C to stop."

wait
