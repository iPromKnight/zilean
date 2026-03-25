# Build Stage
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS base
ARG TARGETARCH
WORKDIR /build
COPY . .
RUN dotnet restore -a $TARGETARCH
WORKDIR /build/src/Zilean.ApiService
RUN dotnet publish -c Release --no-restore -a $TARGETARCH -o /app/out
WORKDIR /build/src/Zilean.Scraper
RUN dotnet publish -c Release --no-restore -a $TARGETARCH -o /app/out

# Run Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine
RUN echo "https://dl-cdn.alpinelinux.org/alpine/v3.18/main" > /etc/apk/repositories && \
    echo "https://dl-cdn.alpinelinux.org/alpine/v3.18/community" >> /etc/apk/repositories && \
    apk update
RUN apk add --update --no-cache \
    python3=~3.11 \
    py3-pip=~23.1 \
    curl \
    git \
    icu-libs \
    && ln -sf python3 /usr/bin/python
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV PYTHONUNBUFFERED=1
ENV ZILEAN_PYTHON_PYLIB=/usr/lib/libpython3.11.so.1.0
ENV ASPNETCORE_URLS=http://+:8181

WORKDIR /app
VOLUME /app/data
COPY --from=base /app/out .
COPY --from=base /build/requirements.txt .
RUN rm -rf /app/python || true && \
    mkdir -p /app/python || true
RUN pip3 install -r /app/requirements.txt -t /app/python

ENTRYPOINT ["./zilean-api"]
