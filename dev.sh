#!/bin/bash

#colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' #no color

echo -e "${GREEN}Starting AutoSearch development environment...${NC}"

#change to script directory
cd "$(dirname "$0")"

#check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}Error: dotnet is not installed or not in PATH${NC}"
    exit 1
fi

#check if npm is installed
if ! command -v npm &> /dev/null; then
    echo -e "${RED}Error: npm is not installed or not in PATH${NC}"
    exit 1
fi

#function to cleanup background processes
cleanup() {
    echo -e "\n${YELLOW}Shutting down servers...${NC}"
    kill $SERVER_PID 2>/dev/null
    kill $CLIENT_PID 2>/dev/null
    wait $SERVER_PID 2>/dev/null
    wait $CLIENT_PID 2>/dev/null
    echo -e "${GREEN}Shutdown complete${NC}"
    exit 0
}

#trap ctrl-c and call cleanup
trap cleanup SIGINT SIGTERM

#start server in background
echo -e "${GREEN}Starting .NET server...${NC}"
cd autosearch.Server
dotnet watch run > ../server.log 2>&1 &
SERVER_PID=$!
cd ..

#wait a bit for server to start
sleep 3

#start client in background
echo -e "${GREEN}Starting React client...${NC}"
cd autosearch.client
npm run dev > ../client.log 2>&1 &
CLIENT_PID=$!
cd ..

echo -e "${GREEN}Development servers started!${NC}"
echo -e "${YELLOW}Server: http://localhost:5030${NC}"
echo -e "${YELLOW}Client: http://localhost:5173 (check client.log for actual port)${NC}"
echo -e "${YELLOW}Logs: server.log and client.log${NC}"
echo -e "${YELLOW}Press Ctrl+C to stop all servers${NC}"

#wait for processes
wait


