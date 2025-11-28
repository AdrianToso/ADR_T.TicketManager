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
COPY ["ADR_T.TicketManager.Application/ADR_T.TicketManager.Application.csproj", "ADR_T.TicketManager.Application/"]
COPY ["ADR_T.TicketManager.Infrastructure/ADR_T.TicketManager.Infrastructure.csproj", "ADR_T.TicketManager.Infrastructure/"]
COPY ["ADR_T.TicketManager.WebAPI/ADR_T.TicketManager.WebAPI.csproj", "ADR_T.TicketManager.WebAPI/"]

# Restaurar proyectos individualmente
RUN dotnet restore "ADR_T.TicketManager.Core/ADR_T.TicketManager.Core.csproj" \
    -p:EnableSourceLink=false \
    -p:IncludeSourceRevisionInInformationalVersion=false

RUN dotnet restore "ADR_T.TicketManager.Application/ADR_T.TicketManager.Application.csproj" \
    -p:EnableSourceLink=false \
    -p:IncludeSourceRevisionInInformationalVersion=false

RUN dotnet restore "ADR_T.TicketManager.Infrastructure/ADR_T.TicketManager.Infrastructure.csproj" \
    -p:EnableSourceLink=false \
    -p:IncludeSourceRevisionInInformationalVersion=false

RUN dotnet restore "ADR_T.TicketManager.WebAPI/ADR_T.TicketManager.WebAPI.csproj" \
    -p:EnableSourceLink=false \
    -p:IncludeSourceRevisionInInformationalVersion=false

# Copiar resto del código
COPY . .

# Construir específicamente el proyecto API
WORKDIR "/src/ADR_T.TicketManager.WebAPI"
RUN dotnet build "ADR_T.TicketManager.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build \
    -p:EnableSourceLink=false \
    -p:EnableSourceControlManagerQueries=false \
    -p:IncludeSourceRevisionInInformationalVersion=false \
    -p:Deterministic=true

# Etapa de publicación
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ADR_T.TicketManager.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish \
    /p:UseAppHost=false \
    -p:EnableSourceLink=false \
    -p:EnableSourceControlManagerQueries=false \
    -p:IncludeSourceRevisionInInformationalVersion=false

# Etapa final
FROM base AS final
WORKDIR /app
RUN mkdir -p logs
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ADR_T.TicketManager.WebAPI.dll"]