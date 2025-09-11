# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Imagem base para o runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Imagem para build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os arquivos de projeto e restaura as dependências
COPY ["ScanDrive.Api/ScanDrive.Api.csproj", "ScanDrive.Api/"]
COPY ["ScanDrive.Domain/ScanDrive.Domain.csproj", "ScanDrive.Domain/"]
COPY ["ScanDrive.Infrastructure/ScanDrive.Infrastructure.csproj", "ScanDrive.Infrastructure/"]
RUN dotnet restore "ScanDrive.Api/ScanDrive.Api.csproj"

# Copia todo o código fonte
COPY . .

# Build da aplicação
WORKDIR "/src/ScanDrive.Api"
RUN dotnet build "ScanDrive.Api.csproj" -c Release -o /app/build

# Publicação
FROM build AS publish
RUN dotnet publish "ScanDrive.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagem final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .


# Script de inicialização
COPY ["ScanDrive.Api/entrypoint.sh", "/app/entrypoint.sh"]
RUN chmod +x /app/entrypoint.sh

ENTRYPOINT ["/app/entrypoint.sh"]