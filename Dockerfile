# Estágio de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os arquivos de projeto
COPY ["ScanDrive.Api/ScanDrive.Api.csproj", "ScanDrive.Api/"]
COPY ["ScanDrive.Domain/ScanDrive.Domain.csproj", "ScanDrive.Domain/"]
COPY ["ScanDrive.Infrastructure/ScanDrive.Infrastructure.csproj", "ScanDrive.Infrastructure/"]

# Restaura as dependências
RUN dotnet restore "ScanDrive.Api/ScanDrive.Api.csproj"

# Copia todo o código fonte
COPY . .

# Build da aplicação
WORKDIR "/src/ScanDrive.Api"
RUN dotnet build "ScanDrive.Api.csproj" -c Release -o /app/build

# Publicação
FROM build AS publish
RUN dotnet publish "ScanDrive.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagem final da API
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS api
WORKDIR /app
COPY --from=publish /app/publish .

# Copia o certificado self-signed para dentro do container
COPY ScanDrive.Api/certs/aspnetcore-devcert.pfx /app/as
