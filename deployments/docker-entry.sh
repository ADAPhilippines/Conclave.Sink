#!/bin/bash

# Start the first process
ASPNETCORE_URLS="http://*:1337" dotnet /bin/TeddySwap.Sink.dll &

# /ipc/node.socket
mkdir -p /ipc
ln -s $CARDANO_NODE_SOCKET_PATH /ipc/node.socket

# Wait for Dotnet to startup
sleep 5

# Start the second process
/bin/oura daemon --config /config/oura.toml &
  
# Wait for any process to exit
wait -n
  
# Exit with status of process that exited first
exit $?