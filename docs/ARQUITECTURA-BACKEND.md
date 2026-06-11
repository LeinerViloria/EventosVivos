# Arquitectura del Backend — EventosVivos

> Documento de socialización. Todas las decisiones aquí descritas fueron tomadas
> de forma argumentada a partir de las **fuerzas** del problema (reglas de negocio,
> concurrencia, TDD) y no por convención. Ver el enunciado en [`ENUNCIADO.md`](./ENUNCIADO.md).

---

## 1. Stack

| Componente | Tecnología |
|------------|------------|
| Runtime / Framework | **.NET Core 10.0** (ASP.NET Core) |
| Base de datos | **PostgreSQL 18** + EF Core |
| Caché / permisos | **Redis** (solo permisos, nunca stock) |
| Mensajería | **RabbitMQ** (bus de eventos oficial) |
| Validación | **FluentValidation** |
| Mediación | **martinothamar/Mediator** (MIT, source-generated) |
| Documentación API | **OpenAPI nativo** (`Microsoft.AspNetCore.OpenApi`) + **Scalar** |
| Generación de PDF | **QuestPDF** (licencia Community) |
| Contenerización | **Docker** (multi-stage) |
| Calidad | **SonarQube** + **CI/CD** |

> **Convención de idioma:** todo el código, carpetas, clases, métodos y variables van en
> **inglés**. Solo la documentación va en español.

---

## 2. Estilo arquitectónico

**Clean Architecture (4 capas, regla de dependencia hacia adentro) + Vertical Slices dentro de la capa Application + un Domain con comportamiento (no anémico).**

### ¿Por qué esta combinación?

El problema tiene un **dominio rico en invariantes** (RN01–RN07: capacidad, solapamiento de venues, horario nocturno, reserva tardía, límites por transacción, autocompletado, cancelación con penalización) y **máquinas de estado** reales (reserva: `pending_payment → confirmed → cancelled/lost`; evento: `active → cancelled/completed`). Eso exige un dominio con lógica, no entidades anémicas.

- **Clean Architecture** aporta la **testabilidad** (dominio puro, infraestructura detrás de abstracciones) y la separación que evita el acoplamiento a EF Core / Redis / RabbitMQ.
- **Vertical Slices** aportan **cohesión por caso de uso**: cada requerimiento funcional es una carpeta autocontenida, evitando los "servicios Dios".
- **Domain con comportamiento** es donde viven las invariantes RN01–RN07, probadas en aislamiento.

### Alternativas descartadas

| Alternativa | Por qué no |
|-------------|-----------|
| N-capas clásica (Controllers → Services → Repositories) | Los servicios se vuelven enormes y las invariantes se dispersan; difícil de testear en aislamiento. |
| Vertical Slice puro (sin capa de dominio) | RN02 y la capacidad cruzan slices; sin dominio compartido se duplican invariantes. |
| DDD táctico completo / CQRS con bases separadas / event sourcing | Sobreingeniería para el alcance y el plazo. Se toma de DDD solo lo que paga: agregados con invariantes y máquinas de estado. |

---

## 3. Estructura de proyectos

El repositorio es un **monorepo**: backend y frontend conviven bajo `src/`.

```
src/
  backend/
    EventosVivos.Domain/          # Entidades con comportamiento, Value Objects, Enums (byte),
                                  #   invariantes RN01-RN07, excepciones de dominio, interfaces de repos
    EventosVivos.Application/     # Slices por feature: Command/Query + Handler + Validator + Endpoint + Response
                                  #   interfaces de servicios (IClock, IPdfGenerator, IPermissionStore, IUnitOfWork)
    EventosVivos.Infrastructure/  # EF Core DbContext + migraciones + seeding, repos, Redis, RabbitMQ,
                                  #   QuestPDF, JWT, Clock (UTC), Outbox
    EventosVivos.Api/             # Endpoints (Minimal APIs), middleware (TZ-header, auth,
                                  #   exception→ProblemDetails), composición DI, Scalar
    tests/
      EventosVivos.Domain.Tests/        # TDD del núcleo: rápidos, sin infraestructura (el grueso de los tests)
      EventosVivos.Application.Tests/   # Handlers con repos en memoria / fakes
      EventosVivos.Api.Tests/           # Integración: WebApplicationFactory + Postgres real (Testcontainers)
  frontend/                       # Aplicación Angular 22 (ver ARQUITECTURA-FRONTEND.md)
```

### Organización por slice

Cada caso de uso es una carpeta autocontenida:

```
Application/Features/Reservations/CreateReservation/
    CreateReservationCommand.cs
    CreateReservationHandler.cs
    CreateReservationValidator.cs    # FluentValidation: quantity >= 1, email format
    CreateReservationEndpoint.cs     # implements IEndpoint → mapea POST /api/v1/reservations
    CreateReservationResponse.cs
```

Áreas: `Events`, `Reservations`, `Reports`, `Auth`.

**Mapa de conceptos del dominio (español → inglés):**
reserva → `Reservation` · evento → `Event` · venue → `Venue` · entrada → `Ticket` · pago → `Payment` · comprador → `Buyer`.

---

## 4. Mediación y pipeline

**`martinothamar/Mediator`** (MIT, generado por source-generators). Se descarta **MediatR** por su cambio a licencia comercial.

El endpoint no conoce al handler; despacha un `Command`/`Query` y el mediator resuelve el handler. Esto sostiene los Vertical Slices y habilita **pipeline behaviors** reutilizables que interceptan toda petición:

```
mediator.Send(command)
   → ValidationBehavior     # corre los FluentValidators antes del handler
   → LoggingBehavior        # trazas y métricas uniformes
   → TransactionBehavior    # transacción + SaveChanges + commit/rollback + reintento por concurrencia
   → Handler
```

---

## 5. Manejo de errores

Se distinguen **dos tipos de error**:

- **Fallos de negocio esperados** → `Result<T>` (sin excepción). Ej.: "reserva ya confirmada", "no hay entradas disponibles", "evento inicia en menos de 1 hora".
- **Fallas excepcionales** → excepciones (BD caída, Redis inalcanzable). Suben y las atrapa un único middleware.

### Contrato de error

Todo error viaja con un **código estable** que el frontend traduce con i18n. **El backend nunca envía texto para el usuario final.**

```jsonc
409 {
  "type": "...", "title": "...", "status": 409,
  "errorCode": "RESERVATION_ALREADY_CONFIRMED",   // ← el front captura esto
  "errorKind": "business",                          // "business" | "general"
  "params": { }                                     // valores dinámicos para interpolar
}
```

- **`errorCode` es `string`** (no enum): es un identificador de contrato que mapea 1:1 a una clave i18n.
- **`params`** lleva los valores dinámicos para que i18n interpole. Ej.:
  - `MAX_TICKETS_PER_TRANSACTION_EXCEEDED` → `{ "max": 5 }`
  - `NOT_ENOUGH_TICKETS` → `{ "requested": 10, "available": 3 }`
- **Validación** → `422` con una **lista** de `{ field, errorCode, params }`.

El middleware traduce todo a **ProblemDetails (RFC 7807)**. Las excepciones no controladas devuelven `500` genérico sin filtrar detalles internos (criterio de **seguridad**).

> El **catálogo de códigos** es un contrato compartido: constantes en el backend ↔ claves en `es-CO.json` del frontend.

---

## 6. Concurrencia y control de overselling

El dolor #1 del negocio es vender más entradas que la capacidad. Validar con un simple `if (disponibles >= cantidad)` **no basta** bajo concurrencia. Se resuelve en dos capas:

### Capa 1 — Concurrencia optimista con `xmin` (INAMOVIBLE)

`Event` es el **agregado raíz**; su versión optimista (`xmin` de Postgres mapeado como rowversion en EF Core) protege la capacidad. Dos reservas concurrentes chocan; la perdedora obtiene `DbUpdateConcurrencyException` y **reintenta** (dentro del `TransactionBehavior`), releyendo el estado actualizado.

> **Redis se reserva exclusivamente para permisos, nunca para el stock.** Una sola fuente de verdad transaccional (Postgres) hace imposible el descuadre entre reserva y stock.

### Capa 2 — Ciclo de vida del recurso (bloqueo y liberación)

"Bloquear" una entrada **no es un lock de BD**: es un estado del dominio. Se usa un **contador denormalizado en `Event`** (`ReservedTickets` / `AvailableTickets`), protegido por `xmin`.

| Estado de la reserva | ¿Retiene entrada? | Evento que lo dispara |
|---|---|---|
| `pending_payment` | **Sí** (bloqueo) | Crear reserva (RF-03) |
| `confirmed` | **Sí** (vendida) | Confirmar pago (RF-04) |
| `cancelled` | **No → libera** | Cancelación (RF-05) |
| `lost` | **Sí, no libera** | Cancelación < 48h de evento confirmado (RN07) |
| `expired` | **No → libera** | Expiración del bloqueo (ver §7) |

```
disponibles = capacidad − Σ(quantity WHERE estado ∈ {pending_payment, confirmed, lost})
```

El bloqueo ocurre al **crear** la reserva (desde `pending_payment`), no al confirmar; de lo contrario habría oversell durante la ventana de pago.

### Solape de horarios por venue (RN02)

La regla RN02 establece que dos eventos activos no pueden compartir el mismo venue con horarios superpuestos. Este es un problema de concurrencia distinto del overselling, pero **se resuelve con el mismo mecanismo de concurrencia optimista con `xmin`**, aplicado esta vez sobre el agregado que gobierna la agenda: el `Venue`.

La diferencia está en que el solape no surge al actualizar una fila existente, sino al insertar dos eventos nuevos a la vez para el mismo venue. Como son filas nuevas, no comparten ninguna versión sobre la cual chocar. Por eso, al crear un evento se **incrementa la versión del `Venue`** correspondiente. Dos creaciones concurrentes sobre el mismo venue leen la misma versión, ambas intentan guardar, una gana y la otra obtiene `DbUpdateConcurrencyException`. La perdedora reintenta dentro del `TransactionBehavior`, vuelve a leer el estado, ahora ve el evento recién creado y rechaza la operación con el código de error `VENUE_SCHEDULE_OVERLAP`.

De esta manera, el solape de horarios queda protegido por la misma estrategia que la capacidad: concurrencia optimista con `xmin` y reintento, sobre el `Venue` para RN02 igual que sobre el `Event` para el control de entradas.

---

## 7. Expiración de reservas y máquinas de estado

Una reserva `pending_payment` que nunca se paga retendría entradas indefinidamente (inventario fantasma). Solución manejada **100% por el backend**:

- La reserva nace con `ExpiresAt = now(UTC) + TTL`.
- Un **`BackgroundService`** barre las `pending_payment` vencidas → las pasa a `expired` y libera (`ReservedTickets--`, protegido por `xmin`).
- **RN06** (autocompletado de eventos cuando se supera la hora de fin) reutiliza el mismo barredor + bus de eventos.

---

## 8. Mensajería: RabbitMQ como bus oficial + patrón Outbox

**RabbitMQ es el bus de eventos de integración oficial del backend.** La expiración/liberación es el primer caso de uso; RN06 y otros eventos de dominio lo reutilizan.

### Patrón Outbox (entrega garantizada)

Requisitos: **nunca publicar un evento si la transacción de negocio falló**, y **reintentar si la publicación falla**. Solo el Outbox cumple ambos:

```
TX {  cambio de negocio (estado=expired, ReservedTickets--)
   +  INSERT en OutboxMessages (evento TicketsReleased)  }  → COMMIT
                                                                ↓
OutboxPublisher (BackgroundService) → lee pendientes → publica a RabbitMQ
                                    → marca Sent | reintenta con backoff
```

- **`OutboxMessages`**: `Id` (Guid v7), `Type`, `Payload`, `OccurredOnUtc`, `Status`, `RetryCount`, `ProcessedOnUtc`.
- El evento se persiste en la **misma transacción** que el cambio → imposible publicar algo que no se confirmó (Caso A).
- El evento persistido **sobrevive a una caída del proceso** → reintentos garantizados (Caso B).
- Entrega **at-least-once** → los **consumidores son idempotentes** (deduplicación por `Id`).

---

## 9. Notificación en tiempo real al frontend: SSE

El front mantiene abierto **un endpoint del backend** y escucha el stream → **Server-Sent Events (SSE)** con `EventSource`. Es server→cliente puro (justo lo que se necesita), sobre HTTP normal, con reconexión automática nativa.

```
Sweeper → expire + release (xmin) → domain event
        → (tras COMMIT) → Outbox → RabbitMQ → consumer
        → push al stream SSE
GET /api/v1/events/stream (SSE) ──keep-alive──> Angular EventSource → UI actualiza disponibilidad
```

> **No se usa SignalR/WebSocket** — se eligió SSE por simplicidad y por encajar con "escuchar un endpoint".

**Cuidados:**
- `EventSource` no permite header `Authorization` → el token de identidad viaja por **query string** (se descartó la cookie por sobreingeniería para el MVP). Los filtros de seguridad son el TTL corto y la firma del token, sumados a que es solo de identidad y a que la sesión es revocable contra Redis.
- Detrás de Nginx: `proxy_buffering off` + timeouts largos.

---

## 10. Endpoints (Minimal APIs)

**Minimal APIs**, un `IEndpoint` por slice (vive junto a su Command/Handler), autodescubiertos por reflexión vía `MapEndpoints()`. Se agrupan con `MapGroup` + extensiones, dando la misma organización y políticas compartidas que un controlador, sin su ceremonia.

```csharp
var v1 = app.MapGroup("/api/v1");
var events       = v1.MapGroup("/events").WithTags("Events").RequireAuthorization();
var reservations = v1.MapGroup("/reservations").WithTags("Reservations");
var reports      = v1.MapGroup("/reports").WithTags("Reports");
```

- **Versionado por ruta**: `/api/v1` (route group raíz). Si luego se requiere versionado por header/query → `Asp.Versioning.Http`.
- **Endpoint delgado**: recibe request → `mediator.Send` → traduce `Result` a HTTP.
- **Documentación**: OpenAPI nativo (`Microsoft.AspNetCore.OpenApi`, **no** Swashbuckle) + **Scalar** (`MapScalarApiReference()`). La agrupación en Scalar se logra con `WithTags` por área.

### Paginación de listados

Como regla general, todo endpoint que devuelve un listado se pagina en el servidor, no solo el de eventos (RF-02). La petición recibe la página y el tamaño de página, la base de datos resuelve la paginación (sin traer todos los registros a memoria) y la respuesta entrega una sola página junto con los metadatos de paginación, como la página actual, el tamaño y el total de elementos.

---

## 11. Validaciones — dónde vive cada una

| Tipo | Dónde | Ejemplos |
|------|-------|----------|
| **Entrada / estructural** | FluentValidation, en el `ValidationBehavior` del pipeline | RF-01 (título 5–100, descripción 10–500), formato de email, cantidad ≥ 1, tipos válidos |
| **Invariantes de negocio** | Dominio | RN01–RN07 |
| **Reglas que consultan otros registros** | Handler / dominio con acceso a repos (**nunca** en un `Validator`) | RN02 (solapamiento de venues), capacidad disponible |

---

## 12. Decisiones transversales

- **Tiempos en UTC**: el backend opera y persiste **solo en UTC**. El frontend envía la zona horaria del cliente en un **header** por petición; la conversión a hora local es responsabilidad del front.
- **Opciones cerradas como `enum : byte`**: estado de evento, estado de reserva, tipo de evento, rol. El contrato de API viaja con el **valor numérico**; la traducción de etiquetas la hace i18n en el front.
- **Migraciones controladas por el backend**: EF Core migrations versionadas (no `EnsureCreated`).
- **Datos semilla controlados e idempotentes**. Se siembran únicamente datos de referencia y de acceso, nunca datos transaccionales:
  - **Venues**: los tres lugares de referencia del enunciado (Auditorio Central con capacidad 200 en Bogotá, Sala Norte con capacidad 50 en Bogotá y Arena Sur con capacidad 500 en Medellín).
  - **Usuarios**: al menos un administrador y un usuario común. Como el enunciado no incluye registro de usuarios, sin esta semilla no habría forma de iniciar sesión ni de probar la aplicación de extremo a extremo.
  - **Catálogo de permisos por rol**: la definición de qué puede hacer cada rol, sembrada como configuración en Redis (ver sección 13).
  - Los eventos y las reservas no se siembran: son datos transaccionales que se crean desde la aplicación. Tampoco hay tablas de catálogo para estados, tipos ni roles, porque se modelan como `enum : byte`.
- **PK = Guid v7** (nativo de Postgres 18, ordenable temporalmente → mejor para índices que Guid v4).

---

## 13. Autenticación y autorización

La autenticación se basa en JWT y maneja dos roles, `admin` y `usuario`, representados como `enum : byte`. Al iniciar sesión, el backend emite dos tokens con propósitos distintos. El **token de identidad** viaja en cada petición (`Authorization: Bearer`) para identificar al usuario y transporta, además del identificador del usuario y su rol, el identificador de la sesión. El **token de permisos** vive en el frontend y solo se usa para decidir qué mostrar u ocultar en la interfaz.

> **La interfaz no es la fuente de autorización.** La autorización real se aplica en el backend, que valida los permisos contra **Redis** en cada petición. Aunque se manipule el token de permisos en el cliente, el backend rechaza la acción.

### Modelo de permisos en Redis (dos niveles)

La validación se apoya en dos estructuras que se complementan:

1. **Catálogo `rol → permisos`**: define qué puede hacer cada rol. Es único para todo el sistema, por lo que un cambio en los permisos de un rol se refleja de inmediato para todos los usuarios. Por su tamaño reducido se puede sembrar como configuración.
2. **Clave por sesión** (`sessionId → { userId, role, issuedAt }`) con tiempo de vida definido. Permite revocar el acceso de un usuario concreto de forma inmediata, eliminándola, y vincula el token de identidad con una sesión viva.

En cada petición el backend verifica primero que la sesión siga existiendo en Redis, lo que actúa como control de revocación, y luego resuelve los permisos efectivos del usuario a partir de su rol consultando el catálogo. Así, los cambios en los permisos de un rol se aplican al instante para todos y, al mismo tiempo, es posible revocar a un usuario individual sin esperar a que caduque su token.

El mapeo entre cada endpoint y el permiso que requiere se aplica con políticas en los `MapGroup`. Por ejemplo, confirmar el pago de una reserva (RF-04) queda restringido al rol `admin`.

---

## 14. Estrategia de pruebas (TDD)

La arquitectura es **testable por diseño**: el dominio es puro y la infraestructura está detrás de abstracciones.

| Proyecto | Alcance | Velocidad |
|----------|---------|-----------|
| `Domain.Tests` | Invariantes RN01–RN07, máquinas de estado. Sin infraestructura. **El grueso de los tests.** | Muy rápidos |
| `Application.Tests` | Handlers con repos en memoria / fakes | Rápidos |
| `Api.Tests` | Integración con `WebApplicationFactory` + Postgres real (Testcontainers) | Más lentos |

---

## Resumen de decisiones

| # | Decisión | Elección |
|---|----------|----------|
| Estilo | Arquitectura | Clean + Vertical Slices + Domain con comportamiento |
| a | Mediación | `martinothamar/Mediator` con pipeline behaviors |
| b | Errores | `Result<T>` + `errorCode` string + `params` → ProblemDetails |
| c | Overselling | Concurrencia optimista `xmin` (inamovible) + reintento |
| — | Inventario | Contador denormalizado en `Event` |
| — | Expiración | `BackgroundService` (backend) |
| — | Mensajería | RabbitMQ (bus oficial) + patrón Outbox |
| — | Tiempo real | SSE (no SignalR) |
| d | Endpoints | Minimal APIs `/api/v1` + Scalar |
| e | Validaciones | FluentValidation (entrada) / Dominio (invariantes) |
