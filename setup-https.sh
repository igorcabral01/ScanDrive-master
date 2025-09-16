#!/bin/bash

# Script para configurar HTTPS na EC2
echo "🚀 Configurando HTTPS para ScanDrive na EC2..."

# 1. Criar diretório SSL
mkdir -p ssl

# 2. Instalar Certbot (Let's Encrypt) - no host EC2
sudo apt update
sudo apt install -y certbot

# 3. Gerar certificados SSL (substitua SEU_DOMINIO.com pelo seu domínio)
echo "📝 Para gerar certificados SSL, execute:"
echo "sudo certbot certonly --standalone -d SEU_DOMINIO.com"
echo ""
echo "Depois copie os certificados:"
echo "sudo cp /etc/letsencrypt/live/SEU_DOMINIO.com/fullchain.pem ./ssl/cert.pem"
echo "sudo cp /etc/letsencrypt/live/SEU_DOMINIO.com/privkey.pem ./ssl/key.pem"
echo "sudo chown $USER:$USER ./ssl/*.pem"
echo ""

# 4. Rodar com HTTPS
echo "🌐 Para rodar com HTTPS:"
echo "docker-compose -f docker-compose.https.yml up -d"
echo ""
echo "📍 Acessos:"
echo "HTTP:  http://SEU_DOMINIO.com (redireciona para HTTPS)"
echo "HTTPS: https://SEU_DOMINIO.com"
echo "Swagger: https://SEU_DOMINIO.com/swagger"
