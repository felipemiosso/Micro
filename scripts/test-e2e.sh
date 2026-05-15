#!/bin/bash

API_PORT=5249
WEB_PORT=4200

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
  kill $API_PID $WEB_PID 2>/dev/null
  stop_port $API_PORT
  stop_port $WEB_PORT
}

# Cleanup before start
stop_port $API_PORT
stop_port $WEB_PORT

echo "Starting Backend (Micro.API)..."
export ASPNETCORE_ENVIRONMENT=Testing
dotnet run --project src/Micro.API --urls=http://localhost:$API_PORT &
API_PID=$!

echo "Starting Frontend (Micro.Web)..."
cmd //c "npm.cmd start --prefix src/Micro.Web -- --port $WEB_PORT" &
WEB_PID=$!

echo "Waiting for servers to be ready..."
# Wait for API
while ! curl -s http://localhost:$API_PORT/health > /dev/null; do
  sleep 1
done
echo "Backend is ready."

# Wait for Web
while ! curl -s http://localhost:$WEB_PORT > /dev/null; do
  sleep 1
done
echo "Frontend is ready."

echo "Running E2E tests..."
dotnet test tests/Micro.E2E/Micro.E2E.csproj
TEST_EXIT_CODE=$?

cleanup

exit $TEST_EXIT_CODE
