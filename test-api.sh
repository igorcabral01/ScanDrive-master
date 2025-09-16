#!/bin/bash

# Script para testar a ScanDrive API
echo "🧪 Testando ScanDrive API..."

# Verificar se está rodando
echo "📊 Verificando containers..."
docker ps | grep scandrive

echo ""
echo "🔍 Testando endpoints..."

# 1. Health Check
echo "1️⃣ Health Check:"
curl -s http://localhost/api/monitoring/health | jq '.' 2>/dev/null || curl -s http://localhost/api/monitoring/health

echo ""
echo ""

# 2. Swagger
echo "2️⃣ Swagger UI:"
response=$(curl -s -o /dev/null -w "%{http_code}" http://localhost/swagger)
if [ "$response" = "200" ]; then
    echo "✅ Swagger acessível em http://localhost/swagger"
else
    echo "❌ Swagger não acessível (HTTP $response)"
fi

echo ""

# 3. Testar alguns endpoints da API
echo "3️⃣ Testando endpoints da API:"

# Auth endpoint
echo "🔐 Auth endpoint:"
curl -s -o /dev/null -w "Status: %{http_code}\n" http://localhost/api/auth/test 2>/dev/null || echo "Endpoint não disponível ou requer autenticação"

# Vehicles endpoint
echo "🚗 Vehicles endpoint:"
curl -s -o /dev/null -w "Status: %{http_code}\n" http://localhost/api/vehicles 2>/dev/null || echo "Endpoint não disponível ou requer autenticação"

# Monitoring endpoint
echo "📊 Monitoring endpoint:"
curl -s -o /dev/null -w "Status: %{http_code}\n" http://localhost/api/monitoring/health

echo ""
echo "🎯 Para acessar:"
echo "• API: http://localhost"
echo "• Swagger: http://localhost/swagger"
echo "• Health: http://localhost/api/monitoring/health"
