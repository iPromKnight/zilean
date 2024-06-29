# Build Stage

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
COPY . /build
WORKDIR /build/src/Zilean.ApiService
RUN dotnet publish -c Release -o /build/out -r linux-$TARGETARCH --self-contained false

# Run Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine3.19

WORKDIR /app

COPY --from=build /build/out .

RUN addgroup -S zilean && adduser -S -G zilean zilean

RUN mkdir /app/data && chown -R zilean:zilean /app/data

USER zilean

HEALTHCHECK --interval=30s --timeout=30s --start-period=5s --retries=3 \
  CMD pgrep -f dotnet || exit 1

VOLUME /app/data

ENTRYPOINT ["dotnet", "Zilean.ApiService.dll"]