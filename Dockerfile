# Build Stage
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
WORKDIR /build
COPY . .
WORKDIR /build/src/Zilean.ApiService
RUN dotnet restore -a $TARGETARCH
RUN dotnet publish -c Release --no-restore -o /build/out -a $TARGETARCH /p:AssemblyName=zilean

# Run Stage
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:8.0
COPY docker/scripts/change_uid_gid.sh /usr/local/bin/change_uid_gid.sh
RUN chmod +x /usr/local/bin/change_uid_gid.sh
ENV ZILEAN_UID=1000
ENV ZILEAN_GID=1000
RUN addgroup --system --gid $ZILEAN_GID zilean && adduser --system --uid $ZILEAN_UID --gid $ZILEAN_GID zilean
USER zilean
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8181
VOLUME /app/data
COPY --from=build /build/out .
ENTRYPOINT ["/usr/local/bin/change_uid_gid.sh"]
CMD ["./zilean"]