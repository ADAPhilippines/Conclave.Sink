FROM mcr.microsoft.com/dotnet/sdk:7.0 AS dotnet-builder
COPY src/ /build/src
WORKDIR /build/src/TeddySwap.Sink

RUN dotnet restore
RUN dotnet publish -c Release -o /build/bin

FROM rust:1.67.1 AS rust-builder
WORKDIR /
ARG CACHE_BUST=1
RUN git clone https://github.com/ADAPhilippines/oura.git
WORKDIR /oura
RUN cargo install --all-features --path .

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /bin
COPY --from=dotnet-builder /build/bin .
COPY --from=rust-builder /usr/local/cargo/bin .
COPY deployments/config /config
COPY deployments/docker-entry.sh /docker-entry.sh
RUN chmod 755 /docker-entry.sh
ENTRYPOINT /docker-entry.sh
