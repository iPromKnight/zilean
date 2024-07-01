#!/bin/sh

if [ $(id -u zilean) -ne $PUID ]; then
    usermod -u $PUID zilean
fi

if [ $(id -g zilean) -ne $PGID ]; then
    groupmod -g $PGID zilean
fi

chown -R zilean:zilean /app

exec "$@"