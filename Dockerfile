# Build Stage
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
WORKDIR /build
COPY . .
WORKDIR /build/src/Zilean.ApiService
RUN dotnet restore -a $TARGETARCH
RUN dotnet publish -c Release --no-restore -o /build/out -a $TARGETARCH /p:AssemblyName=zilean

# Run Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8181
VOLUME /app/data
COPY --from=build /build/out .
ENTRYPOINT ["./zilean"]