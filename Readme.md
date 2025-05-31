# ADR_T.TicketManager - Sistema de Gestión de Tickets

Este repositorio contiene la solución completa para el sistema de gestión de tickets, `ADR_T.TicketManager`. El proyecto está construido sobre .NET 9, siguiendo los principios de Clean Architecture y el patrón CQRS, y está diseñado para ser escalable y mantenible.

Actualmente, la solución soporta dos interfaces de usuario desacopladas: una API RESTful para un frontend (no incluido en este repositorio como parte del backend) y un microservicio de notificaciones. La integración con Docker Compose permite un entorno de desarrollo local rápido y consistente.

## Tabla de Contenidos

1.  [Visión General del Proyecto](#1-visión-general-del-proyecto)
2.  [Microservicios y Tecnologías Clave](#2-microservicios-y-tecnologías-clave)
3.  [Estructura de la Solución](#3-estructura-de-la-solución)
4.  [Requisitos Previos](#4-requisitos-previos)
5.  [Configuración del Entorno de Desarrollo con Docker Compose](#5-configuración-del-entorno-de-desarrollo-con-docker-compose)
    * [5.1. Configuración de Variables de Entorno](#51-configuración-de-variables-de-entorno)
    * [5.2. Levantando los Servicios con Docker Compose](#52-levantando-los-servicios-con-docker-compose)
    * [5.3. Verificación de Servicios](#53-verificación-de-servicios)
6.  [Acceso a las Interfaces de Usuario y Herramientas](#6-acceso-a-las-interfaces-de-usuario-y-herramientas)
7.  [Consideraciones de Desarrollo (HTTPS en Docker)](#7-consideraciones-de-desarrollo-https-en-docker)
8.  [Cómo Ejecutar los Proyectos sin Docker (Alternativa)](#8-cómo-ejecutar-los-proyectos-sin-docker-alternativa)
    * [8.1. Configuración de Base de Datos y Migraciones](#81-configuración-de-base-de-datos-y-migraciones)
    * [8.2. Configuración de RabbitMQ](#82-configuración-de-rabbitmq)
    * [8.3. Iniciar Proyectos Individualmente](#83-iniciar-proyectos-individualmente)
9.  [Documentación Adicional](#9-documentación-adicional)
10. [Contribución](#10-contribución)

---

### 1. Visión General del Proyecto

El sistema `ADR_T.TicketManager` es una aplicación de gestión de tickets diseñada para demostrar y aplicar activamente principios de **Clean Architecture y el patrón CQRS (Command Query Responsibility Segregation)** dentro de un entorno de microservicios. Su propósito principal es gestionar el ciclo de vida de los tickets (creación, asignación, actualización de estado, etc.) y sus notificaciones asociadas.

Más allá de su funcionalidad básica, este proyecto sirve como un **laboratorio práctico para el desarrollo de habilidades** en tecnologías clave del ecosistema .NET y DevOps, incluyendo:

* **ASP.NET Core 9**: Desarrollo de APIs robustas y microservicios.
* **Inyección de Dependencias**: Gestión eficiente y desacoplada de los componentes de la aplicación.
* **Entity Framework Core**: Gestión de la persistencia de datos.
* **MassTransit y RabbitMQ**: Implementación de comunicación asíncrona y procesamiento de eventos.
* **Docker y Docker Compose**: Orquestación de entornos de desarrollo y producción consistentes.
* **Prácticas de Clean Architecture y CQRS**: Diseño de software desacoplado, escalable y mantenible.

### 2. Microservicios y Tecnologías Clave

Este proyecto se compone de varios microservicios y utiliza las siguientes tecnologías:

* **ADR_T.TicketManager.WebAPI**: API principal para la gestión de tickets.
    * Tecnologías: ASP.NET Core (.NET 9.0), MediatR (CQRS), Entity Framework Core, JWT Authentication, Serilog, **Inyección de Dependencias**.
* **ADR_T.NotificationService.API**: Microservicio encargado del envío de notificaciones.
    * Tecnologías: ASP.NET Core (.NET 9.0), Entity Framework Core, Serilog, MassTransit (para comunicación con RabbitMQ), **Inyección de Dependencias**.
* **SQL Server**: Base de datos relacional para la persistencia de datos de ambos servicios.
* **RabbitMQ**: Broker de mensajes utilizado por MassTransit para la comunicación asíncrona entre servicios (ej. publicación/consumo de eventos).
* **Apache Kafka / Confluent Kafka**: Plataforma de streaming de eventos (si bien no está directamente integrada con los servicios .NET aún, se incluye en el entorno Docker para futura expansión).
* **Zookeeper**: Coordinador de servicios necesario para Kafka.
* **Kafka UI**: Interfaz de usuario para monitorear y gestionar Kafka.
* **Docker / Docker Compose**: Herramientas esenciales para la orquestación, contenedorización y ejecución local de todos los servicios.

### 3. Estructura de la Solución

La solución está organizada en múltiples proyectos, cada uno representando una capa en la arquitectura limpia:

#### Proyectos Principales (Ticket Manager)

* **`ADR_T.TicketManager.Core`**: (Dominio) Contiene las entidades del dominio, interfaces de repositorios, excepciones, constantes y eventos de dominio. Es el corazón de la lógica de negocio.
* **`ADR_T.TicketManager.Application`**: (Aplicación) Define la lógica de negocio específica de la aplicación. Incluye DTOs, comandos (Commands), consultas (Queries) y sus respectivos manejadores (Handlers) que implementan el patrón CQRS (utilizando MediatR). También define contratos para servicios de aplicación.
* **`ADR_T.TicketManager.Infrastructure`**: (Infraestructura) Contiene las implementaciones concretas de las interfaces definidas en `Core` y `Application`. Esto incluye la configuración de la persistencia (Entity Framework Core para `AppDbContext`), repositorios, servicios de identidad (ASP.NET Core Identity) y la implementación del bus de eventos (RabbitMQ). Aquí se configuran las dependencias para su inyección.
* **`ADR_T.TicketManager.WebAPI`**: (Presentación - API) Es el proyecto de la API RESTful que expone los endpoints. Actúa como un adaptador que consume los comandos y consultas de la capa `Application`. Es el punto de entrada principal donde se registran y resuelven las dependencias.
* **`ADR_T.TicketManager.Web` (Próximamente/En Desarrollo)**: (Presentación - MVC/Razor) Un nuevo proyecto ASP.NET Core MVC que contendrá las vistas Razor y controladores para una interfaz de usuario tradicional basada en servidor. Consumirá también la capa `Application`.
* **`ADR_T.TicketManager.Tests`**: Contiene los proyectos de pruebas unitarias para las diferentes capas de la solución, asegurando la calidad y el comportamiento esperado del código.

#### Microservicio de Notificaciones

* **`ADR_T.NotificationService.Domain`**: Contiene la entidad de dominio `NotificationLog` y otros elementos específicos del microservicio de notificaciones.
* **`ADR_T.NotificationService.Application`**: Define los consumidores de eventos (MassTransit) para procesar eventos de dominio de otros servicios (ej. `TicketCreadoEvent`, `TicketActualizadoEvent`).
* **`ADR_T.NotificationService.Infrastructure`**: Implementa la persistencia para los logs de notificaciones (Entity Framework Core para `NotificationDbContext`) y la configuración de MassTransit para el microservicio de notificaciones. Aquí también se configuran las dependencias del servicio.
* **`ADR_T.NotificationService.API`**: Es la aplicación principal (Host) que ejecuta el microservicio de notificaciones, configurando los consumidores y la infraestructura necesaria, incluyendo el registro de dependencias.

### 4. Requisitos Previos

Asegúrate de tener instalados los siguientes componentes en tu sistema:

* **Docker Desktop**: Imprescindible, ya que incluye Docker Engine y Docker Compose.
    * [Descargar Docker Desktop](https://www.docker.com/products/docker-desktop/)
* **(Opcional) .NET SDK 9.0**: Recomendado si necesitas ejecutar pruebas o trabajar con los proyectos fuera de los contenedores Docker.
    * [Descargar .NET](https://dotnet.microsoft.com/download)
* **(Opcional) SQL Server Management Studio (SSMS)**: Para administrar y visualizar las bases de datos de SQL Server que se ejecutan en Docker.
    * [Descargar SSMS](https://docs.microsoft.com/es-es/sql/ssms/download-sql-server-management-studio-ssms)
* **Un cliente de Git**: Para clonar el repositorio.

### 5. Configuración del Entorno de Desarrollo con Docker Compose

Este es el método **recomendado** para ejecutar el proyecto en tu entorno de desarrollo.

#### 5.1. Configuración de Variables de Entorno

Crea un archivo llamado `.env` en la **raíz del proyecto** (al mismo nivel que `docker-compose.yml`) y añade las siguientes variables. Estas serán utilizadas por Docker Compose para configurar los servicios:
Variables para SQL Server
SQL_SA_PASSWORD=YourStrongPassword!123 # ¡IMPORTANTE! Cambia esto a una contraseña segura y recuérdala.

Variables para RabbitMQ
RABBITMQ_DEFAULT_USER=user
RABBITMQ_DEFAULT_PASS=password

**Nota de seguridad**: El archivo `.env` contiene credenciales sensibles y **NO DEBE SER SUBIDO A REPOSITORIOS PÚBLICOS**. Asegúrate de que tu `.gitignore` incluye la línea `.env`.

#### 5.2. Levantando los Servicios con Docker Compose

1.  Navega a la **raíz del proyecto** en tu terminal (donde se encuentra `docker-compose.yml`).
2.  Ejecuta el siguiente comando para construir las imágenes Docker y levantar todos los servicios en modo *detached* (segundo plano):

    ```bash
    docker compose up -d --build
    ```
    Este comando realizará varias acciones:
    * Construirá la imagen de SQL Server utilizando el `Dockerfile` personalizado en `sqlserver_image/`.
    * Construirá las imágenes para `webapi` y `notificationservice` a partir de sus respectivos `Dockerfiles`.
    * Descargará las imágenes pre-construidas para Zookeeper, Kafka, RabbitMQ y Kafka UI.
    * Levantará y conectará todos los contenedores.

#### 5.3. Verificación de Servicios

Después de ejecutar `docker compose up -d --build`, puedes verificar el estado de los servicios y sus logs:

* **Estado de los contenedores:**
    ```bash
    docker compose ps
    ```
    Deberías ver todos los servicios en estado `running` (o `healthy` para `sqlserver`).
* **Revisar logs de servicios específicos:**
    ```bash
    docker compose logs webapi
    docker compose logs notificationservice
    # Puedes revisar otros servicios si es necesario:
    # docker compose logs sqlserver
    # docker compose logs rabbitmq
    # docker compose logs kafka
    ```
    En los logs de `webapi` y `notificationservice`, busca líneas como `Application started`, `Now listening on`, o `Bus started: rabbitmq://rabbitmq/` para confirmar que se iniciaron correctamente y se conectaron a RabbitMQ. Para `sqlserver`, busca `Healthy` en el estado de `docker compose ps` y mensajes de inicialización en los logs.

### 6. Acceso a las Interfaces de Usuario y Herramientas

Una vez que todos los servicios estén en marcha con Docker Compose, puedes acceder a sus interfaces y herramientas:

* **ADR_T.TicketManager.WebAPI Swagger UI**: `http://localhost:7000/swagger`
* **ADR_T.NotificationService.API Swagger UI**: `http://localhost:7002/swagger`
* **Kafka UI**: `http://localhost:8080`
* **RabbitMQ Management**: `http://localhost:15672`
    * Credenciales por defecto: `user` / `password` (o las definidas en tu `.env`).
* **SQL Server**: Accesible desde SQL Server Management Studio (SSMS) en `localhost,1433`.
    * Autenticación: SQL Server Authentication
    * Login: `SA`
    * Password: La contraseña definida en tu `.env` (ej. `YourStrongPassword!123`).
    * Bases de datos esperadas: `ADR_T_TicketManagerDb` y `ADR_T_NotificationDb`.

### 7. Consideraciones de Desarrollo (HTTPS en Docker)

Para optimizar el desarrollo local con Docker y evitar problemas con certificados SSL/TLS, la `ADR_T.TicketManager.WebAPI` está configurada para ejecutarse **solo sobre HTTP** dentro del contenedor:

* La línea `app.UseHttpsRedirection();` en `ADR_T.TicketManager.WebAPI/Program.cs` ha sido comentada.
* En `docker-compose.yml`, la configuración para el servicio `webapi` se ha ajustado para:
    * Eliminar el mapeo del puerto HTTPS (`7001:443`).
    * Configurar la variable de entorno `ASPNETCORE_URLS` a `http://+:80`, instruyendo a Kestrel a escuchar solo en HTTP en el puerto 80.
    * El puerto `7000` en el host está mapeado al puerto `80` del contenedor para la WebAPI (`7000:80`).

Para entornos de producción o escenarios donde se requiera HTTPS, se recomienda encarecidamente utilizar un proxy inverso (como Nginx o Caddy) para manejar la terminación SSL/TLS externa a los contenedores de la aplicación.

### 8. Cómo Ejecutar los Proyectos sin Docker (Alternativa)

Si prefieres ejecutar los proyectos directamente en tu máquina local sin Docker Compose (requerirá instalaciones manuales de SQL Server, RabbitMQ, etc.):

1.  **Restaurar Dependencias y Compilar la Solución:**
    Asegúrate de tener el .NET SDK 9.0 (o superior) instalado.
    Abre una terminal de Powershell en la raíz de la solución (`ADR_T.TicketManager`).
    ```powershell
    dotnet restore
    dotnet build
    ```
    Si tienes problemas de compilación después de un renombrado o cambios importantes, ejecuta:
    ```powershell
    dotnet clean
    dotnet restore
    dotnet build
    ```

#### 8.1. Configuración de Base de Datos y Migraciones

* Asegúrate de tener una instancia de SQL Server localmente accesible.
* Las cadenas de conexión se configuran en `appsettings.json` de los proyectos `ADR_T.TicketManager.WebAPI` y `ADR_T.NotificationService.API`. Asegúrate de actualizarlas para que apunten a tu instancia local de SQL Server.
* Para aplicar las migraciones de la base de datos (después de configurar las cadenas de conexión):
    ```powershell
    # Asegúrate de tener la herramienta 'dotnet ef' instalada: dotnet tool install --global dotnet-ef

    # Para la base de datos de Ticket Manager
    cd ADR_T.TicketManager.Infrastructure
    dotnet ef database update
    cd ..

    # Para la base de datos de Notificaciones
    cd ADR_T.NotificationService.Infrastructure
    dotnet ef database update
    cd ..
    ```

#### 8.2. Configuración de RabbitMQ

* Asegúrate de tener una instancia de RabbitMQ en ejecución y accesible localmente.
* Verifica las configuraciones de conexión a RabbitMQ en `appsettings.json` de `ADR_T.TicketManager.WebAPI` y `ADR_T.NotificationService.API`.

#### 8.3. Iniciar Proyectos Individualmente

Abre terminales separadas para cada proyecto que desees ejecutar:

1.  **Iniciar la WebAPI (Backend Principal):**
    ```powershell
    cd ADR_T.TicketManager.WebAPI
    dotnet run
    ```
    La API estará disponible en la URL configurada en su `launchSettings.json` (ej. `https://localhost:7000`).

2.  **Iniciar el Microservicio de Notificaciones:**
    ```powershell
    cd ADR_T.NotificationService.API
    dotnet run
    ```
    Este servicio escuchará los eventos publicados en RabbitMQ.

### 9. Documentación Adicional

Para un análisis más profundo de la arquitectura, patrones de diseño y flujos de datos del proyecto, consulta el documento:
* [Análisis_Detallado_del_Proyecto.md](Análisis_Detallado_del_Proyecto.md)

### 10. Contribución

Se anima a la contribución. Por favor, asegúrate de seguir las directrices de Clean Architecture y buenas prácticas de codificación.