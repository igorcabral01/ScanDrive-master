#!/bin/bash

# Espera o MySQL iniciar
echo "Waiting for MySQL to start..."
sleep 10

# Cria o banco de dados se não existir
mysql -u root -pscandrive123 -e "CREATE DATABASE IF NOT EXISTS scandrive;"

# Configurações adicionais do MySQL
mysql -u root -pscandrive123 -e "SET GLOBAL max_connections = 1000;"
mysql -u root -pscandrive123 -e "SET GLOBAL wait_timeout = 28800;"
mysql -u root -pscandrive123 -e "SET GLOBAL interactive_timeout = 28800;"

echo "Database initialization completed!" 