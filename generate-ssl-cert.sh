#!/bin/bash

# Script para gerar certificado .pfx para ASP.NET Core
echo "🔐 Gerando certificado .pfx para ASP.NET Core..."

# 1. Obter certificados Let's Encrypt
sudo certbot certonly --standalone -d $1

# 2. Converter para .pfx
openssl pkcs12 -export \
  -out ./ssl/cert.pfx \
  -inkey /etc/letsencrypt/live/$1/privkey.pem \
  -in /etc/letsencrypt/live/$1/fullchain.pem \
  -password pass:

echo "✅ Certificado .pfx criado em ./ssl/cert.pfx"
echo "📝 Use docker-compose -f docker-compose.prod.yml up -d"
