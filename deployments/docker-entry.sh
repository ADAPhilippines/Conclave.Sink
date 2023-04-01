#!/bin/bash

# Start the first process
ASPNETCORE_URLS="http://*:1337" dotnet /bin/TeddySwap.Sink.dll &


# Wait for Dotnet to startup
sleep 10

# Start the second process
/bin/oura daemon --config /config/oura.$CARDANO_NODE_MAGIC.toml &
  
# Wait for any process to exit
wait -n
  
# Exit with status of process that exited first
exit $?
