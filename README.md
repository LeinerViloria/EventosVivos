# EventosVivos

Núcleo del sistema de reservas de **EventosVivos**, una startup que organiza eventos culturales, conferencias y talleres. El sistema resuelve el control de capacidad en tiempo real, la gestión de conflictos de horario en los lugares y el ciclo de vida de las reservas y sus pagos. El enunciado completo del problema está en [`docs/ENUNCIADO.md`](./docs/ENUNCIADO.md).

> Este README se actualiza de forma incremental a medida que avanza el desarrollo. Las
> instrucciones de ejecución reflejan el montaje previsto con Docker Compose.

---

## Tecnologías utilizadas

**Backend**
- .NET Core 10.0 (ASP.NET Core, Minimal APIs)
- Entity Framework Core sobre PostgreSQL 18
- Redis (validación de permisos) y RabbitMQ (bus de eventos)
- FluentValidation, `martinothamar/Mediator`, QuestPDF
- OpenAPI nativo con Scalar para la documentación de la API

**Frontend**
- Angular 22.0.0 (signals, modo zoneless, Signal Forms)
- PrimeNG y Tailwind para la interfaz
- Transloco para internacionalización (`es-CO`)
- Vitest y Angular Testing Library para las pruebas

**Infraestructura y calidad**
- Docker y Docker Compose
- SonarQube y CI/CD

---

## Arquitectura

El repositorio es un **monorepo**: el backend y el frontend conviven bajo `src/`, en `src/backend` y `src/frontend`.

### Backend

El backend adopta **Clean Architecture con Vertical Slices y un dominio con comportamiento**. Clean Architecture aporta la separación de responsabilidades y la testabilidad, manteniendo el dominio puro y la infraestructura detrás de abstracciones. Los Vertical Slices organizan el código por caso de uso, lo que da cohesión y evita los servicios sobrecargados. El dominio con comportamiento es donde viven las invariantes de negocio (las reglas RN01 a RN07) y las máquinas de estado de eventos y reservas.

Las decisiones más relevantes son el control de concurrencia con bloqueo optimista mediante `xmin` de PostgreSQL, que evita la sobreventa de entradas y los solapamientos de horario; el bus de eventos con RabbitMQ apoyado en el patrón Outbox para garantizar la entrega; las notificaciones en tiempo real mediante Server-Sent Events; y un contrato de errores basado en códigos que el frontend traduce. La justificación detallada está en [`docs/ARQUITECTURA-BACKEND.md`](./docs/ARQUITECTURA-BACKEND.md).

### Frontend

El frontend se organiza **por features, con manejo de estado mediante signals y stores basados en servicios**, sin una librería de estado externa, siguiendo la estrategia recomendada para Angular 22 y adecuada al alcance del proyecto. La capa de datos usa `httpResource` para las lecturas reactivas y `HttpClient` para los comandos, los formularios se construyen con Signal Forms y la interfaz se compone con componentes reutilizables sobre PrimeNG y Tailwind. La justificación detallada está en [`docs/ARQUITECTURA-FRONTEND.md`](./docs/ARQUITECTURA-FRONTEND.md).

### Convenciones de trabajo

Las convenciones transversales del proyecto (desarrollo guiado por pruebas, manejo de errores, internacionalización, tiempos en UTC, seguridad, acceso a datos, modelado de base de datos, flujo de desarrollo y calidad) están documentadas en la carpeta [`docs/skills/`](./docs/skills/README.md).

---

## Cómo ejecutar el proyecto localmente

### Requisitos

- Docker y Docker Compose

### Pasos

1. Clonar el repositorio:
   ```bash
   git clone <url-del-repositorio>
   cd EventosVivos
   ```
2. Levantar el conjunto completo de servicios:
   ```bash
   docker compose up --build
   ```

Esto inicia la API, la aplicación web, PostgreSQL, Redis y RabbitMQ. El backend aplica las migraciones y los datos semilla de forma controlada al arrancar, de modo que los lugares de referencia y los usuarios iniciales quedan disponibles sin pasos adicionales.

### Servicios

Una vez iniciados los contenedores, los servicios quedan disponibles en las direcciones definidas por el archivo de Docker Compose, que incluyen la aplicación web, la API y la documentación de la API generada con Scalar. Las direcciones y credenciales concretas se documentan en esta sección a medida que se construye el proyecto.

---

## Documentación

- [`docs/ENUNCIADO.md`](./docs/ENUNCIADO.md) — enunciado del problema.
- [`docs/ARQUITECTURA-BACKEND.md`](./docs/ARQUITECTURA-BACKEND.md) — arquitectura del backend y su justificación.
- [`docs/ARQUITECTURA-FRONTEND.md`](./docs/ARQUITECTURA-FRONTEND.md) — arquitectura del frontend y su justificación.
- [`docs/skills/`](./docs/skills/README.md) — convenciones técnicas del proyecto.
