#!/bin/bash

# 🚀 Script completo para deploy da ScanDrive na EC2 com HTTPS
# Execute este script na sua instância EC2

echo "🚀 Configurando ScanDrive na EC2 com HTTPS..."
echo "================================================"

# Verificar se está rodando como root ou com sudo
if [ "$EUID" -eq 0 ]; then
    echo "⚠️  Não execute como root. Use um usuário comum com sudo."
    exit 1
fi

# Variáveis (ALTERE CONFORME NECESSÁRIO)
DOMAIN=${1:-"seudominio.com"}
EMAIL=${2:-"seu@email.com"}
DB_PASSWORD=${3:-"scandrive123"}

echo "📋 Configuração:"
echo "• Domínio: $DOMAIN"
echo "• Email: $EMAIL"
echo "• Senha DB: $DB_PASSWORD"
echo ""

read -p "Confirma as configurações acima? (s/n): " confirm
if [ "$confirm" != "s" ] && [ "$confirm" != "S" ]; then
    echo "❌ Configuração cancelada."
    echo "Use: bash deploy-ec2.sh seudominio.com seu@email.com senhadb123"
    exit 1
fi

echo ""
echo "🔧 Iniciando configuração..."

# 1. ATUALIZAR SISTEMA
echo "1️⃣ Atualizando sistema..."
sudo apt update && sudo apt upgrade -y

# 2. INSTALAR DEPENDÊNCIAS
echo "2️⃣ Instalando dependências..."
sudo apt install -y curl wget git unzip

# 3. INSTALAR DOCKER
echo "3️⃣ Instalando Docker..."
if ! command -v docker &> /dev/null; then
    curl -fsSL https://get.docker.com -o get-docker.sh
    sudo sh get-docker.sh
    sudo usermod -aG docker $USER
    echo "⚠️  Docker instalado. Você precisa fazer logout/login para usar Docker sem sudo."
    echo "Execute: newgrp docker"
    newgrp docker
else
    echo "✅ Docker já instalado"
fi

# 4. INSTALAR DOCKER COMPOSE
echo "4️⃣ Instalando Docker Compose..."
if ! command -v docker-compose &> /dev/null; then
    sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
    sudo chmod +x /usr/local/bin/docker-compose
else
    echo "✅ Docker Compose já instalado"
fi

# 5. INSTALAR CERTBOT (Let's Encrypt)
echo "5️⃣ Instalando Certbot..."
sudo apt install -y certbot

# 6. CLONAR/ATUALIZAR REPOSITÓRIO
echo "6️⃣ Configurando código..."
if [ ! -d "ScanDrive-master" ]; then
    echo "Clonando repositório..."
    git clone https://github.com/igorcabral01/ScanDrive-master.git
    cd ScanDrive-master
else
    echo "Atualizando repositório..."
    cd ScanDrive-master
    git pull origin main
fi

# 7. CONFIGURAR FIREWALL (Opcional)
echo "7️⃣ Configurando firewall..."
sudo ufw allow ssh
sudo ufw allow 80
sudo ufw allow 443
sudo ufw --force enable

# 8. PARAR POSSÍVEIS SERVIÇOS NA PORTA 80
echo "8️⃣ Parando serviços na porta 80..."
sudo systemctl stop apache2 2>/dev/null || true
sudo systemctl stop nginx 2>/dev/null || true

# 9. GERAR CERTIFICADOS SSL
echo "9️⃣ Gerando certificados SSL..."
sudo certbot certonly --standalone \
    --email $EMAIL \
    --agree-tos \
    --no-eff-email \
    -d $DOMAIN

# 10. CRIAR DIRETÓRIO SSL E COPIAR CERTIFICADOS
echo "🔟 Configurando certificados..."
mkdir -p ssl
sudo cp /etc/letsencrypt/live/$DOMAIN/fullchain.pem ./ssl/cert.pem
sudo cp /etc/letsencrypt/live/$DOMAIN/privkey.pem ./ssl/key.pem
sudo chown $USER:$USER ./ssl/*.pem

# 11. CRIAR ARQUIVO .ENV
echo "1️⃣1️⃣ Criando arquivo de configuração..."
cat > .env << EOF
DOMAIN=$DOMAIN
DB_PASSWORD=$DB_PASSWORD
DOCKER_USERNAME=scandrive
VERSION=latest
EOF

# 12. CONSTRUIR E RODAR COM HTTPS
echo "1️⃣2️⃣ Construindo e rodando aplicação..."
docker-compose -f docker-compose.https.yml up --build -d

# 13. VERIFICAR STATUS
echo "1️⃣3️⃣ Verificando status..."
sleep 10
docker ps

echo ""
echo "🎉 CONFIGURAÇÃO CONCLUÍDA!"
echo "=========================="
echo ""
echo "🌐 Acessos:"
echo "• HTTP:  http://$DOMAIN (redireciona para HTTPS)"
echo "• HTTPS: https://$DOMAIN"
echo "• Swagger: https://$DOMAIN/swagger"
echo "• Health: https://$DOMAIN/api/monitoring/health"
echo ""
echo "📊 Comandos úteis:"
echo "• Ver logs: docker-compose -f docker-compose.https.yml logs -f"
echo "• Parar: docker-compose -f docker-compose.https.yml down"
echo "• Reiniciar: docker-compose -f docker-compose.https.yml restart"
echo ""
echo "🔄 Renovação automática SSL:"
echo "sudo crontab -e"
echo "Adicione: 0 12 * * * /usr/bin/certbot renew --quiet"
echo ""
echo "🧪 Para testar:"
echo "bash test-https.sh $DOMAIN"
