#!/bin/bash

# Configurações
DOCKER_USERNAME="seu-usuario"  # Substitua pelo seu usuário do DockerHub
IMAGE_NAME="scandrive"
VERSION=$(git describe --tags --always --dirty || echo "dev")
LATEST_TAG="latest"
IS_PRIVATE=true  # true para imagem privada, false para pública

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Função para imprimir mensagens coloridas
print_message() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

# Verifica se está logado no DockerHub
if ! docker info | grep -q "Username"; then
    print_error "Você não está logado no DockerHub. Por favor, faça login primeiro com 'docker login'"
    exit 1
fi

# Verifica se o repositório existe
REPO_EXISTS=$(curl -s -o /dev/null -w "%{http_code}" https://hub.docker.com/v2/repositories/${DOCKER_USERNAME}/${IMAGE_NAME}/)

if [ "$REPO_EXISTS" = "404" ]; then
    print_message "Criando novo repositório no DockerHub..."
    # Cria o repositório no DockerHub
    curl -X POST \
        -H "Authorization: Bearer $(cat ~/.docker/config.json | jq -r '.auths."https://index.docker.io/v1/".auth' | base64 -d | cut -d: -f2)" \
        -H "Content-Type: application/json" \
        -d "{\"namespace\": \"${DOCKER_USERNAME}\", \"name\": \"${IMAGE_NAME}\", \"is_private\": ${IS_PRIVATE}}" \
        https://hub.docker.com/v2/repositories/
    
    if [ $? -ne 0 ]; then
        print_error "Falha ao criar repositório no DockerHub"
        exit 1
    fi
else
    # Atualiza a visibilidade do repositório existente
    print_message "Atualizando visibilidade do repositório..."
    curl -X PATCH \
        -H "Authorization: Bearer $(cat ~/.docker/config.json | jq -r '.auths."https://index.docker.io/v1/".auth' | base64 -d | cut -d: -f2)" \
        -H "Content-Type: application/json" \
        -d "{\"is_private\": ${IS_PRIVATE}}" \
        https://hub.docker.com/v2/repositories/${DOCKER_USERNAME}/${IMAGE_NAME}/
    
    if [ $? -ne 0 ]; then
        print_warning "Falha ao atualizar visibilidade do repositório, continuando mesmo assim..."
    fi
fi

# Build da imagem
print_message "Iniciando build da imagem..."
if ! docker build -t ${DOCKER_USERNAME}/${IMAGE_NAME}:${VERSION} -t ${DOCKER_USERNAME}/${IMAGE_NAME}:${LATEST_TAG} .; then
    print_error "Falha ao construir a imagem"
    exit 1
fi

# Push da imagem
print_message "Enviando imagem para o DockerHub..."
if ! docker push ${DOCKER_USERNAME}/${IMAGE_NAME}:${VERSION}; then
    print_error "Falha ao enviar a imagem com tag ${VERSION}"
    exit 1
fi

if ! docker push ${DOCKER_USERNAME}/${IMAGE_NAME}:${LATEST_TAG}; then
    print_error "Falha ao enviar a imagem com tag latest"
    exit 1
fi

print_message "Imagem enviada com sucesso!"
print_message "Tags: ${VERSION} e ${LATEST_TAG}"
print_message "URL: https://hub.docker.com/r/${DOCKER_USERNAME}/${IMAGE_NAME}"
print_message "Visibilidade: $([ "$IS_PRIVATE" = true ] && echo "Privada" || echo "Pública")"

# Opcional: Limpar imagens antigas
read -p "Deseja remover imagens antigas? (s/N) " -n 1 -r
echo
if [[ $REPLY =~ ^[Ss]$ ]]; then
    print_message "Removendo imagens antigas..."
    docker image prune -f
fi 