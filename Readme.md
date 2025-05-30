# ADR_T.TicketManager

Este repositorio contiene la solución completa para el sistema de gestión de tickets, `ADR_T.TicketManager`. El proyecto está construido sobre .NET 9, siguiendo los principios de Clean Architecture y el patrón CQRS, y está diseñado para ser escalable y mantenible.

Actualmente, la solución soporta dos interfaces de usuario desacopladas: una API RESTful para un frontend basado en Angular (no incluido en este repositorio como parte del backend) y una aplicación web basada en Razor Views. Adicionalmente, incluye un microservicio de notificaciones.

## Estructura de la Solución

La solución está organizada en múltiples proyectos, cada uno representando una capa en la arquitectura limpia:

### Proyectos Principales (Ticket Manager)

* **`ADR_T.TicketManager.Core`**: (Dominio) Contiene las entidades del dominio, interfaces de repositorios, excepciones, constantes y eventos de dominio. Es el corazón de la lógica de negocio.
* **`ADR_T.TicketManager.Application`**: (Aplicación) Define la lógica de negocio específica de la aplicación. Incluye DTOs, comandos (Commands), consultas (Queries) y sus respectivos manejadores (Handlers) que implementan el patrón CQRS (utilizando MediatR). También define contratos para servicios de aplicación.
* **`ADR_T.TicketManager.Infrastructure`**: (Infraestructura) Contiene las implementaciones concretas de las interfaces definidas en `Core` y `Application`. Esto incluye la configuración de la persistencia (Entity Framework Core para `AppDbContext`), repositorios, servicios de identidad (ASP.NET Core Identity) y la implementación del bus de eventos (RabbitMQ).
* **`ADR_T.TicketManager.WebAPI`**: (Presentación - API) Es el proyecto de la API RESTful que expone los endpoints para el frontend de Angular. Actúa como un adaptador que consume los comandos y consultas de la capa `Application`.
* **`ADR_T.TicketManager.Web` (Próximamente/En Desarrollo)**: (Presentación - MVC/Razor) Un nuevo proyecto ASP.NET Core MVC que contendrá las vistas Razor y controladores para una interfaz de usuario tradicional basada en servidor. Consumirá también la capa `Application`.
* **`ADR_T.TicketManager.Tests`**: Contiene los proyectos de pruebas unitarias para las diferentes capas de la solución, asegurando la calidad y el comportamiento esperado del código.

### Microservicio de Notificaciones

* **`ADR_T.NotificationService.Domain`**: Contiene la entidad de dominio `NotificationLog` y otros elementos específicos del microservicio de notificaciones.
* **`ADR_T.NotificationService.Application`**: Define los consumidores de eventos (MassTransit) para procesar eventos de dominio de otros servicios (ej. `TicketCreadoEvent`, `TicketActualizadoEvent`).
* **`ADR_T.NotificationService.Infrastructure`**: Implementa la persistencia para los logs de notificaciones (Entity Framework Core para `NotificationDbContext`) y la configuración de MassTransit para el microservicio de notificaciones.
* **`ADR_T.NotificationService.API`**: Es la aplicación principal (Host) que ejecuta el microservicio de notificaciones, configurando los consumidores y la infraestructura necesaria.

## Tecnologías Clave

* **.NET 9 (o superior)**
* **C#**
* **ASP.NET Core MVC / Razor Pages** (para `ADR_T.TicketManager.Web`)
* **ASP.NET Core Web API** (para `ADR_T.TicketManager.WebAPI`)
* **Clean Architecture**
* **CQRS** (con MediatR)
* **Entity Framework Core**
* **ASP.NET Core Identity**
* **MassTransit** (para Event Bus / RabbitMQ)
* **Bootstrap** (para el styling del frontend Razor)

## Configuración del Entorno de Desarrollo

1.  **Clonar el Repositorio:**
    ```bash
    git clone <URL_DEL_REPOSITORIO>
    cd ADR_T.TicketManager
    ```

2.  **Restaurar Dependencias y Compilar la Solución:**
    Asegúrate de tener el .NET SDK 9.0 (o superior) instalado.
    Abre una terminal de Powershell en la raíz de la solución (`ADR_T.TicketManager`).
    ```powershell
    dotnet restore
    dotnet build
    ```
    Si tienes problemas de compilación después de un renombrado, ejecuta:
    ```powershell
    dotnet clean
    dotnet restore
    dotnet build
    ```

3.  **Configuración de Base de Datos:**
    * Los proyectos `ADR_T.TicketManager.Infrastructure` y `ADR_T.NotificationService.Infrastructure` utilizan Entity Framework Core.
    * Las cadenas de conexión se configuran en `appsettings.json` de los proyectos `ADR_T.TicketManager.WebAPI` y `ADR_T.NotificationService.API`. Asegúrate de actualizarlas con tus credenciales de SQL Server.
    * Para aplicar las migraciones de la base de datos (después de configurar las cadenas de conexión):
        ```powershell
        # Para la base de datos de Ticket Manager
        cd ADR_T.TicketManager.Infrastructure
        dotnet ef database update

        # Para la base de datos de Notificaciones
        cd ..\ADR_T.NotificationService.Infrastructure
        dotnet ef database update
        ```
        *Nota: Es posible que necesites instalar la herramienta `dotnet ef` globalmente si aún no lo has hecho: `dotnet tool install --global dotnet-ef`.*

4.  **Configuración de RabbitMQ:**
    * El microservicio de notificaciones y el bus de eventos (`ADR_T.TicketManager.Infrastructure`) utilizan RabbitMQ. Asegúrate de tener una instancia de RabbitMQ en ejecución y configurada correctamente.
    * Verifica las configuraciones de conexión a RabbitMQ en `appsettings.json` de `ADR_T.TicketManager.WebAPI` y `ADR_T.NotificationService.API`.

## Cómo Ejecutar los Proyectos

Puedes ejecutar los proyectos desde Visual Studio o usando el .NET CLI.

1.  **Iniciar la WebAPI (Backend Principal):**
    ```powershell
    cd ADR_T.TicketManager.WebAPI
    dotnet run
    ```
    La API estará disponible en la URL configurada (ej. `https://localhost:7000`).

2.  **Iniciar el Microservicio de Notificaciones:**
    ```powershell
    cd ADR_T.NotificationService.API
    dotnet run
    ```
    Este servicio escuchará los eventos publicados en RabbitMQ.

3.  **Iniciar la Aplicación Web Razor (si ya está configurada):**
    ```powershell
    cd ADR_T.TicketManager.Web
    dotnet run
    ```
    La aplicación web estará disponible en la URL configurada (ej. `https://localhost:7001`).

## Documentación Adicional

Para un análisis más profundo de la arquitectura, patrones de diseño y flujos de datos del proyecto, consulta el documento:
* [Análisis_Detallado_del_Proyecto.md](Análisis_Detallado_del_Proyecto.md)

## Contribución

Se anima a la contribución. Por favor, asegúrate de seguir las directrices de Clean Architecture y buenas prácticas de codificación.