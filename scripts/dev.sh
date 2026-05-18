#!/bin/bash

API_PORT=5249
WEB_PORT=4200
FIREBASE_PORT=9099

stop_port() {
  local port=$1
  local pid=$(netstat -ano | grep LISTENING | grep ":$port " | awk '{print $5}' | head -n 1)
  if [ ! -z "$pid" ]; then
    echo "Stopping process on port $port (PID: $pid)..."
    taskkill //F //PID $pid
  fi
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
firebase emulators:start --only auth &
FIREBASE_PID=$!

echo "Starting Backend (Micro.API)..."
dotnet run --project src/Micro.API --urls=http://localhost:$API_PORT &
API_PID=$!

echo "Waiting for Backend to be ready..."
for i in {1..30}; do
  if curl -s http://localhost:$API_PORT/health > /dev/null; then
    echo "Backend is alive."
    break
  fi
  sleep 1
done

echo "Starting Frontend (Micro.Web)..."
cmd //c "npm.cmd start --prefix src/Micro.Web -- --port $WEB_PORT" &
WEB_PID=$!

echo "Dev environment running. Press Ctrl+C to stop."

wait
