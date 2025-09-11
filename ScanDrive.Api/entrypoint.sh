#!/bin/bash

# Espera o banco de dados estar pronto
echo "Waiting for database..."
sleep 10

# Aplica as migrations
echo "Running migrations..."
dotnet ef database update

# Inicia a aplicação
echo "Starting application..."
dotnet ScanDrive.Api.dll 