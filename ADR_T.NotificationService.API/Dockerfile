# Etapa base para runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar SOLO proyectos necesarios (excluyendo tests)
COPY ["ADR_T.TicketManager.Core/ADR_T.TicketManager.Core.csproj", "ADR_T.TicketManager.Core/"]
COPY ["ADR_T.NotificationService.Domain/ADR_T.NotificationService.Domain.csproj", "ADR_T.NotificationService.Domain/"]
COPY ["ADR_T.NotificationService.Application/ADR_T.NotificationService.Application.csproj", "ADR_T.NotificationService.Application/"]
COPY ["ADR_T.NotificationService.Infrastructure/ADR_T.NotificationService.Infrastructure.csproj", "ADR_T.NotificationService.Infrastructure/"]
COPY ["ADR_T.NotificationService.API/ADR_T.NotificationService.API.csproj", "ADR_T.NotificationService.API/"]

# Restaurar proyectos individualmente
RUN dotnet restore "ADR_T.TicketManager.Core/ADR_T.TicketManager.Core.csproj" \
    -p:EnableSourceLink=false \
    -p:IncludeSourceRevisionInInformationalVersion=false

RUN dotnet restore "ADR_T.NotificationService.Domain/ADR_T.NotificationService.Domain.csproj" \
    -p:EnableSourceLink=false \
    -p:IncludeSourceRevisionInInformationalVersion=false

RUN dotnet restore "ADR_T.NotificationService.Application/ADR_T.NotificationService.Application.csproj" \
    -p:EnableSourceLink=false \
    -p:IncludeSourceRevisionInInformationalVersion=false

RUN dotnet restore "ADR_T.NotificationService.Infrastructure/ADR_T.NotificationService.Infrastructure.csproj" \
    -p:EnableSourceLink=false \
    -p:IncludeSourceRevisionInInformationalVersion=false

RUN dotnet restore "ADR_T.NotificationService.API/ADR_T.NotificationService.API.csproj" \
    -p:EnableSourceLink=false \
    -p:IncludeSourceRevisionInInformationalVersion=false

# Copiar resto del código
COPY . .

# Construir específicamente el proyecto API
WORKDIR "/src/ADR_T.NotificationService.API"
RUN dotnet build "ADR_T.NotificationService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build \
    -p:EnableSourceLink=false \
    -p:EnableSourceControlManagerQueries=false \
    -p:IncludeSourceRevisionInInformationalVersion=false \
    -p:Deterministic=true

# Etapa de publicación
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ADR_T.NotificationService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish \
    /p:UseAppHost=false \
    -p:EnableSourceLink=false \
    -p:EnableSourceControlManagerQueries=false \
    -p:IncludeSourceRevisionInInformationalVersion=false

# Etapa final
FROM base AS final
WORKDIR /app
RUN mkdir -p logs
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ADR_T.NotificationService.API.dll"]