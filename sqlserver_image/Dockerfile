# Usa la imagen base de SQL Server con una versión específica
FROM mcr.microsoft.com/mssql/server:2022-CU19-ubuntu-22.04

# Cambia al usuario root para realizar la instalación de paquetes
USER root

# Paso 1: Actualiza el sistema e instala curl, dirmngr y gnupg
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    dirmngr \
    gnupg \
    && rm -rf /var/lib/apt/lists/*

# Paso 2: Añade los repositorios de Microsoft
# Descarga la clave GPG de Microsoft y la guarda directamente en /etc/apt/trusted.gpg.d/
# Luego, añade la lista de fuentes de Microsoft
RUN curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > /etc/apt/trusted.gpg.d/microsoft.gpg \
    && curl https://packages.microsoft.com/config/ubuntu/22.04/prod.list | tee /etc/apt/sources.list.d/msprod.list \
    && apt-get update

# Paso 3: Configura la aceptación del EULA e instala msodbcsql17 y mssql-tools
# Ya que el repositorio de Microsoft está añadido, apt ahora puede encontrar los paquetes.
RUN export DEBIAN_FRONTEND=noninteractive \
    && ACCEPT_EULA=Y apt-get install -y --no-install-recommends \
    msodbcsql17 \
    mssql-tools \
    && echo 'export PATH="$PATH:/opt/mssql-tools/bin"' >> /etc/profile.d/mssql-tools.sh \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Vuelve al usuario mssql (opcional, la imagen base ya lo maneja)
# USER mssql