#!/bin/bash

# 🚀 Script RÁPIDO para deploy ScanDrive na EC2
# Para quando você já tem Docker instalado

set -e  # Parar se algum comando falhar

echo "⚡ Deploy RÁPIDO da ScanDrive com HTTPS"
echo "======================================"

# Verificar parâmetros
if [ $# -lt 2 ]; then
    echo "❌ Uso: bash quick-deploy.sh DOMINIO EMAIL [SENHA_DB]"
    echo "Exemplo: bash quick-deploy.sh meusite.com admin@meusite.com minhasenha123"
    exit 1
fi

DOMAIN=$1
EMAIL=$2
DB_PASSWORD=${3:-"scandrive123"}

echo "🔧 Configuração:"
echo "• Domínio: $DOMAIN"
echo "• Email: $EMAIL"
echo "• Senha DB: $DB_PASSWORD"
echo ""

# 1. Parar serviços na porta 80
echo "1️⃣ Liberando porta 80..."
sudo systemctl stop apache2 nginx 2>/dev/null || true

# 2. Gerar certificado SSL
echo "2️⃣ Gerando certificado SSL..."
sudo certbot certonly --standalone --email $EMAIL --agree-tos --no-eff-email -d $DOMAIN

# 3. Configurar certificados
echo "3️⃣ Configurando certificados..."
mkdir -p ssl
sudo cp /etc/letsencrypt/live/$DOMAIN/fullchain.pem ./ssl/cert.pem
sudo cp /etc/letsencrypt/live/$DOMAIN/privkey.pem ./ssl/key.pem
sudo chown $USER:$USER ./ssl/*.pem

# 4. Criar .env
echo "4️⃣ Criando configuração..."
cat > .env << EOF
DOMAIN=$DOMAIN
DB_PASSWORD=$DB_PASSWORD
DOCKER_USERNAME=scandrive
VERSION=latest
EOF

# 5. Rodar aplicação
echo "5️⃣ Rodando aplicação..."
docker-compose -f docker-compose.https.yml up --build -d

echo ""
echo "🎉 DEPLOY CONCLUÍDO!"
echo "==================="
echo ""
echo "🌐 Acesse: https://$DOMAIN"
echo "📖 Swagger: https://$DOMAIN/swagger"
echo ""
echo "📊 Status: docker ps"
echo "📝 Logs: docker-compose -f docker-compose.https.yml logs -f"
