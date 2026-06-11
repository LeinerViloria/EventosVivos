# CLAUDE.md — EventosVivos

Núcleo de un sistema de reservas de eventos. Monorepo: `src/backend` (.NET 10) y `src/frontend` (Angular 22, con el código en `src/frontend/app`).

> Este archivo es un índice de reglas. La fuente de verdad detallada está en los documentos
> enlazados al final. Ante una duda real o algo que no sé hacer, **me detengo y pregunto**;
> no improviso ni reabro decisiones ya cerradas.

## Reglas que siempre aplican

### Idioma
- Todo el **código en inglés** (carpetas, clases, métodos, variables, enums, commits).
- Toda la **documentación en español** (`docs/`, README).

### Desarrollo
- **TDD obligatorio** en backend y frontend. Ningún flujo se da por terminado sin pruebas.
- Ejecutar **`dotnet format`** al terminar cada desarrollo en el backend.
- **Una rama por feature**, avance secuencial: no se inicia el siguiente feature hasta integrar el actual a `main` por su pull request, con los pipelines de calidad en verde.

### Backend
- Clean Architecture + Vertical Slices + dominio con comportamiento. Mediator `martinothamar/Mediator` con pipeline behaviors.
- Errores: `Result<T>` para negocio, excepciones para lo excepcional; todo error con `errorCode` **string** + `errorKind` + `params`, traducido a ProblemDetails. El backend **nunca** envía texto de usuario.
- Concurrencia: bloqueo optimista con **`xmin`** + reintento. Sobre `Event` para la capacidad; sobre `Venue` para el solape de horarios (RN02).
- Endpoints: Minimal APIs, un `IEndpoint` por slice, agrupados por `MapGroup`, versionados en `/api/v1`, documentados con **Scalar**.
- Validación: **FluentValidation** para la entrada; el dominio para las invariantes RN01–RN07.
- Mensajería: **RabbitMQ** como bus oficial + patrón **Outbox**. Tiempo real con **SSE** (token de identidad por query string).

### Datos
- **La base de datos resuelve** los cálculos, procesos, búsquedas y la paginación. No traer colecciones a memoria para procesarlas.
- **Sin SQL puro**: solo LINQ sobre EF Core (method o query syntax).
- Modelado con **Fluent API** (`IEntityTypeConfiguration<T>`), **no** Data Annotations.
- Índices con criterio, sin saturar; **sin columnas repetidas entre los índices de una misma tabla**.
- PK = **Guid v7**. Migraciones y datos semilla controlados por el backend. Todo listado se pagina en el servidor.

### Tiempos y enumeraciones
- El backend opera y persiste **solo en UTC**; el frontend envía la zona horaria del cliente en un header.
- Opciones cerradas como **`enum : byte`** en backend y enums numéricos en TypeScript. El contrato viaja con el número; i18n traduce las etiquetas.

### Frontend (Angular 22)
- Por feature, estado con **signals** y stores basados en servicios (sin NgRx).
- Capa de datos: **`httpResource`** para lecturas, **`HttpClient`** para comandos. Errores normalizados a `{ errorCode, errorKind, params }` por interceptor.
- Formularios con **Signal Forms**; los errores de validación usan el `kind` como código del mismo catálogo.
- i18n con **Transloco** (`es-CO`); enums traducidos con un pipe puro.
- Interfaz con **PrimeNG + Tailwind**; **no HTML crudo**, se compone con componentes reutilizables en `shared`.
- Errores en UI: validación en línea (Message de PrimeNG); negocio y generales por Toast.
- Pruebas con **Vitest** y **Angular Testing Library**.

### Autenticación
- JWT con dos tokens: identidad (viaja al backend) y permisos (solo para mostrar/ocultar en la UI).
- La autorización real se aplica en el backend contra **Redis**, con un catálogo `rol → permisos` y una clave por sesión para revocación.

## Documentos de referencia
- `docs/ENUNCIADO.md` — el problema, requerimientos y reglas de negocio.
- `docs/ARQUITECTURA-BACKEND.md` — arquitectura del backend y su justificación.
- `docs/ARQUITECTURA-FRONTEND.md` — arquitectura del frontend y su justificación.
- `docs/skills/` — convenciones técnicas detalladas (una por documento).
