# Usar la imagen base de .NET SDK para construir la aplicación
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copiar archivos .csproj y restaurar dependencias
COPY *.csproj .
RUN dotnet restore

# Copiar el resto del código fuente y construir la aplicación
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# Usar la imagen base de ASP.NET Core runtime para la ejecución
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Configurar el puerto de escucha (debe coincidir con el puerto que usa Kestrel, por defecto 8080 o 80)
# Si tu app escucha en otro puerto, ajusta EXPOSE y ASPNETCORE_URLS
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Entrypoint para ejecutar la aplicación
ENTRYPOINT ["dotnet", "ADR_T.TicketManager.WebAPI.dll"]