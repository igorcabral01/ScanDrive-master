# 📖 GUIA MANUAL: Deploy ScanDrive na EC2 com HTTPS

## 🚀 PASSO-A-PASSO COMPLETO

### 1️⃣ **Conectar na EC2**
```bash
ssh -i sua-chave.pem ubuntu@seu-ip-ec2
```

### 2️⃣ **Atualizar Sistema**
```bash
sudo apt update && sudo apt upgrade -y
```

### 3️⃣ **Instalar Dependências Básicas**
```bash
sudo apt install -y curl wget git unzip
```

### 4️⃣ **Instalar Docker**
```bash
# Baixar e instalar Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Adicionar usuário ao grupo docker
sudo usermod -aG docker $USER

# Ativar o grupo (ou faça logout/login)
newgrp docker

# Testar Docker
docker --version
```

### 5️⃣ **Instalar Docker Compose**
```bash
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# Testar Docker Compose
docker-compose --version
```

### 6️⃣ **Instalar Certbot (Let's Encrypt)**
```bash
sudo apt install -y certbot
```

### 7️⃣ **Configurar Firewall (Opcional)**
```bash
sudo ufw allow ssh
sudo ufw allow 80
sudo ufw allow 443
sudo ufw --force enable
```

### 8️⃣ **Clonar Repositório**
```bash
git clone https://github.com/igorcabral01/ScanDrive-master.git
cd ScanDrive-master
```

### 9️⃣ **Parar Serviços que Usam Porta 80**
```bash
sudo systemctl stop apache2 2>/dev/null || true
sudo systemctl stop nginx 2>/dev/null || true
```

### 🔟 **Gerar Certificados SSL**
```bash
# SUBSTITUA 'seudominio.com' pelo seu domínio real
sudo certbot certonly --standalone \
    --email seu@email.com \
    --agree-tos \
    --no-eff-email \
    -d seudominio.com
```

### 1️⃣1️⃣ **Configurar Certificados**
```bash
# Criar diretório SSL
mkdir -p ssl

# Copiar certificados (SUBSTITUA 'seudominio.com')
sudo cp /etc/letsencrypt/live/seudominio.com/fullchain.pem ./ssl/cert.pem
sudo cp /etc/letsencrypt/live/seudominio.com/privkey.pem ./ssl/key.pem

# Dar permissões
sudo chown $USER:$USER ./ssl/*.pem
```

### 1️⃣2️⃣ **Criar Arquivo de Configuração**
```bash
# Criar arquivo .env
cat > .env << EOF
DOMAIN=seudominio.com
DB_PASSWORD=suasenha123
DOCKER_USERNAME=scandrive
VERSION=latest
EOF
```

### 1️⃣3️⃣ **Construir e Rodar Aplicação**
```bash
# Construir e rodar com HTTPS
docker-compose -f docker-compose.https.yml up --build -d

# Verificar status
docker ps

# Ver logs
docker-compose -f docker-compose.https.yml logs -f
```

### 1️⃣4️⃣ **Testar Aplicação**
```bash
# Testar localmente na EC2
curl -I https://seudominio.com

# Ou usar o script de teste
bash test-https.sh seudominio.com
```

## 🔄 **COMANDOS DE MANUTENÇÃO**

### **Ver Status dos Containers**
```bash
docker ps
docker-compose -f docker-compose.https.yml ps
```

### **Ver Logs**
```bash
# Todos os logs
docker-compose -f docker-compose.https.yml logs -f

# Logs só da API
docker-compose -f docker-compose.https.yml logs -f api

# Logs só do Nginx
docker-compose -f docker-compose.https.yml logs -f nginx
```

### **Reiniciar Aplicação**
```bash
# Reiniciar tudo
docker-compose -f docker-compose.https.yml restart

# Reiniciar só a API
docker-compose -f docker-compose.https.yml restart api
```

### **Parar Aplicação**
```bash
docker-compose -f docker-compose.https.yml down
```

### **Atualizar Código**
```bash
git pull origin main
docker-compose -f docker-compose.https.yml up --build -d
```

## 🔐 **RENOVAÇÃO AUTOMÁTICA SSL**

### **Configurar Cron para Renovação**
```bash
# Editar crontab
sudo crontab -e

# Adicionar linha (roda todo dia às 12h)
0 12 * * * /usr/bin/certbot renew --quiet
```

### **Testar Renovação**
```bash
sudo certbot renew --dry-run
```

## 🧪 **TESTE FINAL**

### **URLs para Testar:**
- **HTTP:** http://seudominio.com (deve redirecionar)
- **HTTPS:** https://seudominio.com
- **Swagger:** https://seudominio.com/swagger
- **Health:** https://seudominio.com/api/monitoring/health

### **Comandos de Teste:**
```bash
# Health check
curl https://seudominio.com/api/monitoring/health

# Verificar redirecionamento HTTP→HTTPS
curl -I http://seudominio.com

# Verificar certificado SSL
echo | openssl s_client -servername seudominio.com -connect seudominio.com:443 | openssl x509 -noout -dates
```

## 🆘 **SOLUÇÃO DE PROBLEMAS**

### **Container não sobe:**
```bash
# Ver logs detalhados
docker-compose -f docker-compose.https.yml logs

# Verificar portas em uso
sudo netstat -tlnp | grep :80
sudo netstat -tlnp | grep :443
```

### **Certificado SSL não funciona:**
```bash
# Verificar se certificados existem
ls -la /etc/letsencrypt/live/seudominio.com/

# Verificar permissões
ls -la ./ssl/
```

### **Domínio não resolve:**
```bash
# Verificar DNS
nslookup seudominio.com
dig seudominio.com
```

## 🎯 **RESUMO DOS COMANDOS PRINCIPAIS**

```bash
# 1. Preparação
sudo apt update && sudo apt upgrade -y
curl -fsSL https://get.docker.com -o get-docker.sh && sudo sh get-docker.sh
sudo usermod -aG docker $USER && newgrp docker

# 2. Código
git clone https://github.com/igorcabral01/ScanDrive-master.git && cd ScanDrive-master

# 3. SSL
sudo apt install -y certbot
sudo certbot certonly --standalone --email seu@email.com --agree-tos -d seudominio.com

# 4. Deploy
mkdir -p ssl
sudo cp /etc/letsencrypt/live/seudominio.com/*.pem ./ssl/
sudo chown $USER:$USER ./ssl/*.pem
docker-compose -f docker-compose.https.yml up --build -d
```
