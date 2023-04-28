FROM mcr.microsoft.com/dotnet/sdk:7.0 AS dotnet-builder
COPY src/ /build/src
WORKDIR /build/src/TeddySwap.Sink.Api
RUN dotnet restore
RUN dotnet publish -c Release -o /build/bin

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /bin
COPY --from=dotnet-builder /build/bin .
ENV ASPNETCORE_URLS="http://*:3000"
ENTRYPOINT ["dotnet", "TeddySwap.Sink.Api.dll"]