# Plan de Desarrollo — EventosVivos

Este documento define el plan de desarrollo del proyecto, organizado por features. Cada feature debe seguir de forma rigurosa la arquitectura y las convenciones ya socializadas; este plan no las reemplaza, sino que indica en qué orden se construyen.

## Principios que rigen todo el desarrollo

Antes de cada feature se tienen presentes, sin excepción, los documentos de referencia:

- La arquitectura del backend, en [`ARQUITECTURA-BACKEND.md`](./ARQUITECTURA-BACKEND.md).
- La arquitectura del frontend, en [`ARQUITECTURA-FRONTEND.md`](./ARQUITECTURA-FRONTEND.md).
- Las convenciones transversales, en la carpeta [`skills/`](./skills/README.md).
- El resumen de reglas siempre activas, en `CLAUDE.md` (en la raíz del repositorio).

Además, el desarrollo respeta tres reglas de proceso. Cada feature se trabaja en **su propia rama**, creada desde `main`. Cada feature entrega **backend y frontend integrados**, de modo que el camino feliz sea ejecutable de extremo a extremo. Y la integración a `main` se hace **por pull request**, que solo se fusiona cuando el pipeline de calidad (formato, pruebas y SonarCloud) queda en verde. Este flujo está descrito en el skill [development-workflow](./skills/development-workflow.md).

## Hoja de ruta

El orden responde a las dependencias entre features. La primera carga el grueso del andamiaje transversal; las siguientes son más delgadas porque reutilizan esa base.

| # | Feature | Rama | Razón del orden | Qué establece, además del requerimiento |
|---|---------|------|-----------------|------------------------------------------|
| 1 | **RF-01 Crear evento** | `feature/events-create-event` | Todo depende de que existan eventos | La base de todo: EF Core y `DbContext`, primera migración, datos semilla de venues, enumeraciones (`EventType`, `EventStatus` como byte), Guid v7, el pipeline del Mediator, FluentValidation, el contrato de errores (`Result` traducido a ProblemDetails), las invariantes RN01, RN02 (concurrencia optimista con `xmin` sobre el `Venue`) y RN03, el endpoint Minimal API y Scalar. En el frontend: la estructura por features, Transloco con `es-CO`, PrimeNG y Tailwind, el **app shell** mínimo (ver más abajo), el formulario con Signal Forms, los interceptores de zona horaria y de normalización de errores, y la presentación de errores. |
| 2 | **RF-02 Listar eventos** | `feature/events-list-events` | Necesita eventos creados | La capa de lectura con `httpResource`, la paginación en el servidor, los filtros (tipo, fecha, venue, estado y búsqueda por título) y la vista de listado con componentes de PrimeNG. |
| 3 | **Autenticación** | `feature/auth` | Protege lo que ya existe | Inicio de sesión, los dos tokens JWT, Redis con el catálogo `rol → permisos` y la clave por sesión, las políticas de autorización en los endpoints y, en el frontend, los guards y el menú según el token de permisos. Se aplica retroactivamente a crear evento, que queda restringido al rol administrador. |
| 4 | **RF-03 Reservar entrada** | `feature/reservations-create-reservation` | Necesita eventos | La entidad `Reservation` y sus estados, el contador `ReservedTickets` en `Event`, la concurrencia optimista con `xmin` y el reintento, las reglas RN04 y RN05 y la regla de las 24 horas, el `ExpiresAt` y el `BackgroundService` de expiración. Aquí se introducen **RabbitMQ con el patrón Outbox** y las notificaciones en vivo por **Server-Sent Events**. |
| 5 | **RF-04 Confirmar pago** | `feature/reservations-confirm-payment` | Necesita reservas (acción de administrador) | La transición a `confirmed`, la generación del código `EV-{6 dígitos}` y el manejo de errores de estado. |
| 6 | **RF-05 Cancelar reserva** | `feature/reservations-cancel-reservation` | Necesita reservas | La liberación de entradas, la regla RN07 (la reserva se marca como perdida si faltan menos de 48 horas) y el registro de la fecha de cancelación. |
| 7 | **RF-06 Reporte de ocupación** | `feature/reports-occupancy` | Necesita eventos y reservas confirmadas | Las agregaciones resueltas en la base de datos, la regla RN06 (autocompletado del evento mediante el barredor) y la exportación a PDF con QuestPDF. |

## Navegación y app shell

La aplicación necesita una estructura de navegación que no corresponde a ningún requerimiento funcional, pero que es indispensable para usarla. Esa estructura se construye de forma progresiva.

Con la primera feature se establece un **app shell mínimo**: un componente de layout con una zona de navegación y un área de contenido gobernada por el `router-outlet`, junto con el enrutamiento de la aplicación. A medida que se agregan features, el **menú crece** con sus opciones, evitando siempre el HTML crudo y componiéndose con los componentes de menú de PrimeNG, según el skill [ui-components](./skills/ui-components.md).

Cuando se incorpore la autenticación, las opciones del menú se **muestran u ocultan según el token de permisos**: por ejemplo, crear un evento y confirmar un pago solo son visibles para el administrador, mientras que el listado de eventos es visible para todos. La autorización real, no obstante, se aplica en el backend, tal como define el skill [security](./skills/security.md).

## Decisiones del plan

La autenticación se ubica en el tercer puesto, después de crear y listar eventos, para contar con funcionalidad visible que proteger y demostrar; no se adelanta al inicio para no retrasar el primer valor entregable. RabbitMQ, el patrón Outbox y los Server-Sent Events se introducen junto con la reserva de entradas (RF-03), que es donde aparece la liberación de entradas y la necesidad de notificar en vivo.

## Punto de partida

El desarrollo comienza por **RF-01, Crear evento**, en la rama `feature/events-create-event`.
