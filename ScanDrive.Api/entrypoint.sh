#!/bin/bash

echo "Waiting for database..."
sleep 10

echo "Running migrations..."
dotnet ef database update

echo "Starting application..."
dotnet ScanDrive.Api.dll
