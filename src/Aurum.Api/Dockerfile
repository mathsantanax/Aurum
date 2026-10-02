# Acesse https://aka.ms/customizecontainer para saber como personalizar seu contêiner de depuração e como o Visual Studio usa este Dockerfile para criar suas imagens para uma depuração mais rápida.

# Esta fase é usada durante a execução no VS no modo rápido (Padrão para a configuração de Depuração)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app

EXPOSE 8080
EXPOSE 8081


# Esta fase é usada para compilar o projeto de serviço
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY ["src/Aurum.Api/Aurum.Api.csproj", "src/Aurum.Api/"]
COPY ["src/Aurum.Application/Aurum.Application.csproj", "src/Aurum.Application/"]
COPY ["src/Aurum.Domain/Aurum.Domain.csproj", "src/Aurum.Domain/"]
COPY ["src/Aurum.Infrastructure/Aurum.Infrastructure.csproj", "src/Aurum.Infrastructure/"]

RUN dotnet restore "src/Aurum.Api/Aurum.Api.csproj"

COPY . .

WORKDIR "/src/src/Aurum.Api"

RUN dotnet build "Aurum.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build \
    --no-restore

# Esta fase é usada para publicar o projeto de serviço a ser copiado para a fase final
FROM build AS publish

ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "Aurum.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false \
    --no-restore

# Esta fase é usada na produção ou quando executada no VS no modo normal (padrão quando não está usando a configuração de Depuração)
FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

USER root

RUN mkdir -p /app/keys && \
    chown -R $APP_UID:$APP_UID /app/keys && \
    chmod 755 /app/keys

USER $APP_UID

ENTRYPOINT ["dotnet", "Aurum.Api.dll"]