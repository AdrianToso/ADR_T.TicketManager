# Ejemplo para ADR_T.NotificationService.API/Dockerfile
# Usando la etiqueta gen�rica "9.0" para .NET 9.0 (base predeterminada de Microsoft)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["ADR_T.NotificationService.API/ADR_T.NotificationService.API.csproj", "ADR_T.NotificationService.API/"]
# Se asume que estos son tus archivos de proyecto y soluci�n.
# Aseg�rate de que las rutas relativas sean correctas desde el contexto de la construcci�n.
COPY ADR_T.TicketManager.sln .
COPY ADR_T.TicketManager.Core/*.csproj ADR_T.TicketManager.Core/
COPY ADR_T.TicketManager.Application/*.csproj ADR_T.TicketManager.Application/
COPY ADR_T.TicketManager.Infrastructure/*.csproj ADR_T.TicketManager.Infrastructure/
COPY ADR_T.TicketManager.WebAPI/*.csproj ADR_T.TicketManager.WebAPI/
COPY ADR_T.TicketManager.Tests/*.csproj ADR_T.TicketManager.Tests/
COPY ADR_T.NotificationService.Domain/*.csproj ADR_T.NotificationService.Domain/
COPY ADR_T.NotificationService.Application/*.csproj ADR_T.NotificationService.Application/
COPY ADR_T.NotificationService.Infrastructure/*.csproj ADR_T.NotificationService.Infrastructure/
COPY ADR_T.NotificationService.API/*.csproj ADR_T.NotificationService.API/
RUN dotnet restore
COPY . .
WORKDIR "/src/ADR_T.NotificationService.API"
RUN dotnet build "ADR_T.NotificationService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ADR_T.NotificationService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ADR_T.NotificationService.API.dll"]