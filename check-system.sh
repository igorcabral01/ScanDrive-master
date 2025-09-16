#!/bin/bash

# 🔍 Script para verificar se tudo está funcionando na EC2

echo "🔍 VERIFICAÇÃO COMPLETA DO SISTEMA"
echo "=================================="
echo ""

# 1. Verificar Docker
echo "1️⃣ Verificando Docker..."
if command -v docker &> /dev/null; then
    echo "✅ Docker instalado: $(docker --version)"
    
    if docker ps &> /dev/null; then
        echo "✅ Docker funcionando"
    else
        echo "❌ Docker não está funcionando. Execute: sudo systemctl start docker"
    fi
else
    echo "❌ Docker não instalado"
fi

echo ""

# 2. Verificar Docker Compose
echo "2️⃣ Verificando Docker Compose..."
if command -v docker-compose &> /dev/null; then
    echo "✅ Docker Compose instalado: $(docker-compose --version)"
else
    echo "❌ Docker Compose não instalado"
fi

echo ""

# 3. Verificar Certbot
echo "3️⃣ Verificando Certbot..."
if command -v certbot &> /dev/null; then
    echo "✅ Certbot instalado: $(certbot --version)"
else
    echo "❌ Certbot não instalado"
fi

echo ""

# 4. Verificar Containers
echo "4️⃣ Verificando Containers..."
if docker ps | grep -q scandrive; then
    echo "✅ Containers ScanDrive rodando:"
    docker ps | grep scandrive
else
    echo "❌ Nenhum container ScanDrive rodando"
    echo "📋 Containers ativos:"
    docker ps
fi

echo ""

# 5. Verificar Portas
echo "5️⃣ Verificando Portas..."
if netstat -tlnp 2>/dev/null | grep -q ":80 "; then
    echo "✅ Porta 80 em uso:"
    netstat -tlnp 2>/dev/null | grep ":80 "
else
    echo "❌ Porta 80 não está em uso"
fi

if netstat -tlnp 2>/dev/null | grep -q ":443 "; then
    echo "✅ Porta 443 em uso:"
    netstat -tlnp 2>/dev/null | grep ":443 "
else
    echo "❌ Porta 443 não está em uso"
fi

echo ""

# 6. Verificar Certificados SSL
echo "6️⃣ Verificando Certificados SSL..."
if [ -d "/etc/letsencrypt/live" ]; then
    echo "✅ Diretório Let's Encrypt existe:"
    sudo ls -la /etc/letsencrypt/live/
else
    echo "❌ Nenhum certificado Let's Encrypt encontrado"
fi

if [ -d "./ssl" ] && [ -f "./ssl/cert.pem" ]; then
    echo "✅ Certificados locais existem:"
    ls -la ./ssl/
else
    echo "❌ Certificados locais não encontrados"
fi

echo ""

# 7. Testar Conectividade Local
echo "7️⃣ Testando Conectividade Local..."
if curl -s -o /dev/null -w "%{http_code}" http://localhost/api/monitoring/health 2>/dev/null | grep -q "200"; then
    echo "✅ API respondendo localmente"
else
    echo "❌ API não responde localmente"
fi

echo ""

# 8. Verificar Logs Recentes
echo "8️⃣ Logs Recentes..."
if docker ps | grep -q scandrive; then
    echo "📝 Últimas 5 linhas dos logs:"
    docker-compose -f docker-compose.https.yml logs --tail=5 2>/dev/null || \
    docker-compose logs --tail=5 2>/dev/null || \
    echo "❌ Não foi possível obter logs"
else
    echo "❌ Nenhum container para verificar logs"
fi

echo ""

# 9. Resumo e Próximos Passos
echo "9️⃣ RESUMO E PRÓXIMOS PASSOS"
echo "============================"

if docker ps | grep -q scandrive && [ -f "./ssl/cert.pem" ]; then
    echo "🎉 Sistema parece estar funcionando!"
    echo ""
    echo "🧪 Para testar externamente:"
    echo "bash test-https.sh SEU_DOMINIO"
    echo ""
    echo "🌐 URLs para testar no navegador:"
    echo "• https://SEU_DOMINIO"
    echo "• https://SEU_DOMINIO/swagger"
else
    echo "⚠️  Sistema precisa de configuração."
    echo ""
    echo "🚀 Para fazer deploy completo:"
    echo "bash deploy-ec2.sh SEU_DOMINIO SEU_EMAIL"
    echo ""
    echo "⚡ Para deploy rápido (se Docker já instalado):"
    echo "bash quick-deploy.sh SEU_DOMINIO SEU_EMAIL"
fi

echo ""
echo "📚 Mais ajuda:"
echo "• Guia completo: cat DEPLOY-EC2-GUIDE.md"
echo "• Ver logs: docker-compose -f docker-compose.https.yml logs -f"
echo "• Status: docker ps"
