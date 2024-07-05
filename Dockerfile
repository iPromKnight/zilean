# Build Stage
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
WORKDIR /build
COPY . .
WORKDIR /build/src/Zilean.ApiService
RUN dotnet restore -a $TARGETARCH
RUN dotnet publish -c Release --no-restore -o /build/out -a $TARGETARCH
WORKDIR /build/src/Zilean.DmmScraper
RUN dotnet restore -a $TARGETARCH
RUN dotnet publish -c Release --no-restore -o /build/out -a $TARGETARCH

# Run Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0

RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_gcServer=1
ENV DOTNET_GCDynamicAdaptationMode=1
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8181
VOLUME /app/data
COPY --from=build /build/out .
ENTRYPOINT ["./zilean-api"]