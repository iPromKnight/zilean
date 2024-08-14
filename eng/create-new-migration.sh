#!/bin/bash

if [ -z "$1" ] || [ -z "$2" ]; then
  echo "Error: Missing arguments."
  echo "Usage: ./create_migration.sh <MigrationName> <CustomPrefix>"
  echo "Example: ./create_migration.sh InitialMigration 123456"
  exit 1
fi

MIGRATION_NAME=$1
CUSTOM_PREFIX=$2

pushd "$(git rev-parse --show-toplevel)/src/Zilean.Database" || exit 1

dotnet ef migrations add "$MIGRATION_NAME"

MIGRATION_FILE=$(find Migrations -name "*_$MIGRATION_NAME.cs" | head -n 1)

if [ -z "$MIGRATION_FILE" ]; then
  echo "Migration file not found."
  exit 1
fi

NEW_FILE_NAME="Migrations/${CUSTOM_PREFIX}_$MIGRATION_NAME.cs"
DESIGNER_FILE="${MIGRATION_FILE%.cs}.Designer.cs"
mv "$MIGRATION_FILE" "$NEW_FILE_NAME"
NEW_DESIGNER_FILE="${NEW_FILE_NAME%.cs}.Designer.cs"
mv "$DESIGNER_FILE" "$NEW_DESIGNER_FILE"

if [[ "$OSTYPE" == "darwin"* ]]; then
  # macOS
  sed -i "" "s/Migration(\"[^\"]*\")/Migration(\"${CUSTOM_PREFIX}_$MIGRATION_NAME\")/" "$NEW_DESIGNER_FILE"
else
  # Linux and others
  sed -i "s/Migration(\"[^\"]*\")/Migration(\"${CUSTOM_PREFIX}_$MIGRATION_NAME\")/" "$NEW_DESIGNER_FILE"
fi

echo "Migration file renamed to ${NEW_FILE_NAME}"
popd || exit 1