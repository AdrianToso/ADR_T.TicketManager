# **Análisis Detallado del Proyecto ADR\_T.TicketManager**

Este documento describe la arquitectura, patrones de diseño y componentes clave de la solución ADR\_T.TicketManager y ADR\_T.NotificationService, siguiendo los principios de Clean Architecture y utilizando tecnologías modernas de .NET.

## **1\. Arquitectura General: Clean Architecture (Arquitectura de Cebolla)**

La solución está diseñada bajo los principios de Clean Architecture (también conocida como Arquitectura de Cebolla), lo que promueve:

* **Separación de Preocupaciones:** Cada capa tiene una responsabilidad específica.  
* **Independencia de Frameworks:** La lógica de negocio central es independiente de frameworks UI, bases de datos o servicios externos.  
* **Testabilidad:** Las capas internas son fácilmente testeables sin depender de la infraestructura.  
* **Escalabilidad y Mantenibilidad:** Facilita la evolución y el crecimiento del sistema.

### **Estructura de Proyectos por Capa:**

* **Dominio (Core):**

  * `ADR_T.TicketManager.Core`: Contiene las entidades del dominio, interfaces de repositorios, excepciones personalizadas, eventos de dominio y otros elementos de lógica de negocio pura que no dependen de ninguna otra capa. Es el corazón de la aplicación.  
  * `ADR_T.NotificationService.Domain`: Similar a `Core`, pero específico para el contexto del microservicio de notificaciones, conteniendo su entidad `NotificationLog`.  
* **Aplicación (Application):**

  * `ADR_T.TicketManager.Application`: Define la lógica de negocio específica de la aplicación. Aquí residen los DTOs (Data Transfer Objects), Comandos (Commands), Consultas (Queries) y sus respectivos manejadores (Handlers). Utiliza MediatR para implementar el patrón CQRS. También define interfaces para servicios de aplicación (ej. `IIdentityService`).  
  * `ADR_T.NotificationService.Application`: Contiene la lógica de aplicación para el microservicio de notificaciones, principalmente los consumidores (Consumers) que procesan mensajes de un bus de eventos (ej. `TicketCreadoEventConsumer`).  
* **Infraestructura (Infrastructure):**

  * `ADR_T.TicketManager.Infrastructure`: Contiene las implementaciones concretas de las interfaces definidas en `Core` y `Application`. Incluye las implementaciones de los repositorios (Entity Framework Core con `AppDbContext`), la configuración de la base de datos, servicios de identidad (`IdentityService` implementando `IIdentityService`), y la integración con el bus de eventos (MassTransit para RabbitMQ). También contiene la implementación de servicios externos.  
  * `ADR_T.NotificationService.Infrastructure`: Implementa la persistencia para el log de notificaciones (`NotificationDbContext`) y la configuración de MassTransit para el microservicio de notificaciones.  
* **Presentación / Adaptadores (Presentation / Adapters):**

  * `ADR_T.TicketManager.WebAPI`: Proyecto ASP.NET Core Web API. Es un adaptador de UI que expone los endpoints RESTful para el frontend de Angular (u otros clientes REST). Los controladores de API envían comandos y consultas a la capa `Application`.  
  * **`ADR_T.TicketManager.Web` (Nueva Adición \- Frontend Razor):** Proyecto ASP.NET Core MVC. Es otro adaptador de UI, coexistiendo con `WebAPI`. Se encarga de recibir solicitudes HTTP, procesar la lógica de presentación a través de sus controladores y renderizar vistas HTML dinámicas utilizando Razor. También consumirá los comandos y consultas de la capa `Application`. **Esta es la nueva capa de presentación que estamos implementando.**  
* **Pruebas (Tests):**

  * `ADR_T.TicketManager.Tests`: Contiene los proyectos de pruebas unitarias e integración para validar el comportamiento de las diferentes capas y componentes.

## **2\. Patrones de Diseño Clave**

### **a. CQRS (Command Query Responsibility Segregation)**

* **Implementación:** Se utiliza la librería **MediatR** en la capa `ADR_T.TicketManager.Application`.  
* **Separación:**  
  * **Comandos (`Commands`):** Objetos que representan una intención de cambio de estado (ej., `CreateTicketCommand`, `LoginUserCommand`). Se envían a través de MediatR y son manejados por `IRequestHandler<TCommand, TResult>`.  
  * **Consultas (`Queries`):** Objetos que representan una solicitud para obtener datos sin cambiar el estado (ej., `GetAllTicketsQuery`, `GetTicketByIdQuery`). También se envían a través de MediatR y son manejados por `IRequestHandler<TQuery, TResult>`.  
* **Beneficios:** Mejora la escalabilidad, mantenibilidad y la comprensión del código al separar las operaciones de lectura de las de escritura, permitiendo optimizaciones independientes.

### **b. Repositorio (Repository)**

* **Implementación:** Interfaces de repositorios (ej., `ITicketRepository`, `IUnitOfWork`) se definen en `ADR_T.TicketManager.Core`.  
* **Abstracción:** Las implementaciones concretas (ej., `TicketRepository`, `UnitOfWork`) residen en `ADR_T.TicketManager.Infrastructure\Persistence`.  
* **Beneficios:** Desacopla la lógica de negocio del mecanismo de persistencia de datos, haciendo que el dominio sea independiente de la tecnología de base de datos (Entity Framework Core en este caso).

### **c. Servicios de Dominio / Aplicación**

* **Servicios de Aplicación:** Clases en `ADR_T.TicketManager.Application` que coordinan la lógica de negocio específica de la aplicación (ej., `IIdentityService` y su implementación en `ADR_T.TicketManager.Infrastructure\Services\IdentityService.cs`).

### **d. Inyección de Dependencias (Dependency Injection \- DI)**

* **Implementación:** Utilizada extensivamente en todo el proyecto, configurada en los métodos `AddApplication` y `AddInfrastructureServices` (métodos de extensión en la capa `Application` e `Infrastructure` respectivamente) y luego en `Program.cs` de los proyectos de presentación (`WebAPI`, `NotificationService.API` y el futuro `Web`).  
* **Beneficios:** Promueve el código modular, testable y flexible al externalizar la creación y gestión de dependencias.

### **e. Event-Driven Architecture (con Message Broker)**

* **Implementación:** Uso de **MassTransit** y **RabbitMQ** para un bus de eventos.  
* **Publicación de Eventos:** Eventos de dominio (ej., `TicketCreadoEvent`, `TicketActualizadoEvent`) pueden ser publicados por la capa de `Application` (a través de `IEventBus`).  
* **Consumo de Eventos:** Los consumidores (ej., en `ADR_T.NotificationService.Application`) se suscriben a estos eventos y realizan acciones en respuesta.  
* **Beneficios:** Permite un desacoplamiento fuerte entre los servicios, mejorando la escalabilidad y la resiliencia al permitir que los servicios reaccionen a los cambios sin estar directamente acoplados.

## **3\. Tecnologías y Herramientas**

* **.NET 9 / C\#:** Plataforma y lenguaje de programación principal.  
* **ASP.NET Core Web API:** Para la construcción de servicios RESTful.  
* **ASP.NET Core MVC:** **(Nueva Adición)** Para la construcción de aplicaciones web con renderizado del lado del servidor usando Razor Views y el patrón Model-View-Controller.  
* **Entity Framework Core:** ORM para la interacción con la base de datos SQL Server (por el uso de `UseSqlServer aunque independientemente se podrían agregar otras implementaciones`).  
* **ASP.NET Core Identity:** Para la gestión de usuarios, roles y autenticación/autorización.  
* **MediatR:** Para la implementación de CQRS.  
* **MassTransit:** Para la orquestación y el manejo de mensajes con RabbitMQ como Message Broker.  
* **xUnit:** Framework para pruebas unitarias.  
* **Moq:** Librería para simular objetos (mocking) en pruebas unitarias.  
* **Bootstrap:** (Nueva Adición para `ADR_T.TicketManager.Web`) Framework CSS/JS para un diseño responsivo y componentes de UI en las vistas Razor.

## **4\. Flujo de Peticiones Típico (WebAPI como ejemplo, aplica similar a Web con adaptaciones)**

1. **Solicitud HTTP (Cliente Angular) \-\> ADR\_T.TicketManager.WebAPI:**

   * Un controlador en `ADR_T.TicketManager.WebAPI` recibe una solicitud (ej., POST a `/api/tickets`).  
   * El controlador deserializa la solicitud a un DTO y lo mapea a un `Command` (ej., `CreateTicketCommand`).  
   * El controlador envía el `Command` al `IMediator` (MediatR).  
2. **`IMediator` \-\> ADR\_T.TicketManager.Application:**

   * MediatR delega el `Command` a su respectivo `IRequestHandler` (ej., `CreateTicketCommandHandler`) en la capa `ADR_T.TicketManager.Application`.  
3. **Lógica de Negocio (ADR\_T.TicketManager.Application):**

   * El `CommandHandler` valida el `Command`, utiliza interfaces de repositorios (`ITicketRepository`, `IUnitOfWork`) para interactuar con el dominio.  
   * Puede publicar eventos de dominio (ej., `TicketCreadoEvent`) a través del `IEventBus` (MassTransit).  
4. **Persistencia (ADR\_T.TicketManager.Infrastructure):**

   * Las implementaciones de repositorios en `ADR_T.TicketManager.Infrastructure\Persistence` interactúan con `AppDbContext` (Entity Framework Core) para guardar cambios en la base de datos.  
   * El `EventBus` (MassTransit) envía los eventos al Message Broker (RabbitMQ).  
5. **Notificación (ADR\_T.NotificationService.API / Application / Infrastructure):**

   * El microservicio `ADR_T.NotificationService.API` tiene un consumidor (ej., `TicketCreadoEventConsumer` en `ADR_T.NotificationService.Application`) que se suscribe a `TicketCreadoEvent` en RabbitMQ.  
   * Este consumidor procesa el evento, lo registra en `NotificationDbContext` (usando `ADR_T.NotificationService.Infrastructure`), y podría realizar otras acciones (ej., enviar un email).  
6. **Respuesta HTTP (ADR\_T.TicketManager.WebAPI \-\> Cliente Angular):**

   * El `CommandHandler` retorna un resultado (ej., `TicketDto`) a MediatR.  
   * MediatR devuelve el resultado al controlador en `ADR_T.TicketManager.WebAPI`.  
   * El controlador serializa el DTO a JSON y lo envía como respuesta al cliente Angular.

## **5\. Flujo de Peticiones Típico (ADR\_T.TicketManager.Web \- Nueva Capa MVC)**

1. **Solicitud HTTP (Navegador) \-\> ADR\_T.TicketManager.Web:**

   * Un controlador en `ADR_T.TicketManager.Web` (ej., `TicketController`) recibe una solicitud (ej., GET a `/Tickets/Create` para mostrar un formulario, o POST a `/Tickets/Create` para enviar datos de formulario).  
   * Si es un POST: El controlador enlaza los datos del formulario a un `ViewModel` y lo mapea a un `Command` (ej., `CreateTicketCommand`).  
   * Si es un GET: El controlador mapea parámetros de la URL a una `Query` (ej., `GetAllTicketsQuery`).  
2. **`IMediator` \-\> ADR\_T.TicketManager.Application:**

   * Similar al flujo de la WebAPI, el controlador de MVC envía el `Command` o `Query` a `IMediator`.  
   * MediatR delega la operación al `IRequestHandler` correspondiente en `ADR_T.TicketManager.Application`.  
3. **Lógica de Negocio y Persistencia:**

   * El flujo en las capas `ADR_T.TicketManager.Application` e `ADR_T.TicketManager.Infrastructure` es idéntico al de la WebAPI, utilizando los mismos `Command` y `Query Handlers`, repositorios y servicios.  
4. **Respuesta de la Capa de Aplicación:**

   * El `CommandHandler` o `QueryHandler` devuelve un resultado (ej., un `TicketDto` o una lista de ellos) a MediatR.  
   * MediatR devuelve el resultado al controlador en `ADR_T.TicketManager.Web`.  
5. **Renderizado de la Vista (ADR\_T.TicketManager.Web \-\> Navegador):**

   * El controlador en `ADR_T.TicketManager.Web` toma el `DTO` o el resultado recibido de la capa `Application`.  
   * Puede mapear este `DTO` a un `ViewModel` específico para la vista.  
   * El controlador selecciona una vista Razor (ej., `Views/Tickets/Create.cshtml` o `Views/Tickets/Index.cshtml`) y le pasa el `ViewModel` como modelo.  
   * La vista Razor renderiza el HTML final, que se envía al navegador del usuario.

