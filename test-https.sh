#!/bin/bash

# Script para testar HTTPS na EC2
echo "🌐 Testando ScanDrive com HTTPS na EC2..."

DOMAIN=${1:-"localhost"}

echo "🔍 Testando domínio: $DOMAIN"
echo ""

# 1. Testar HTTP (deve redirecionar para HTTPS)
echo "1️⃣ Testando HTTP (deve redirecionar):"
curl -I -s http://$DOMAIN

echo ""

# 2. Testar HTTPS
echo "2️⃣ Testando HTTPS:"
curl -I -s https://$DOMAIN

echo ""

# 3. Health Check HTTPS
echo "3️⃣ Health Check via HTTPS:"
curl -s https://$DOMAIN/api/monitoring/health | jq '.' 2>/dev/null || curl -s https://$DOMAIN/api/monitoring/health

echo ""

# 4. Testar certificado SSL
echo "4️⃣ Verificando certificado SSL:"
echo | openssl s_client -servername $DOMAIN -connect $DOMAIN:443 2>/dev/null | openssl x509 -noout -dates

echo ""
echo "🎯 URLs para testar:"
echo "• HTTPS: https://$DOMAIN"
echo "• Swagger: https://$DOMAIN/swagger"
echo "• Health: https://$DOMAIN/api/monitoring/health"

echo ""
echo "📋 Para usar com seu domínio:"
echo "bash test-https.sh seudominio.com"
