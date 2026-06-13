# Sistema de reservas - EventosVivos

Núcleo del sistema de reservas de **EventosVivos**, una startup que organiza eventos culturales, conferencias y talleres. El sistema resuelve el control de capacidad en tiempo real, la gestión de conflictos de horario en los lugares y el ciclo de vida de las reservas y sus pagos. El enunciado completo del problema está en [`docs/ENUNCIADO.md`](./docs/ENUNCIADO.md).

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
2. Crear los archivos de entorno a partir de las plantillas (no se versionan):
   ```bash
   cp src/backend/.env.example src/backend/.env
   cp src/frontend/.env.example src/frontend/.env
   ```
3. Levantar el conjunto completo de servicios:
   ```bash
   docker compose up --build
   ```

Esto inicia la API, la aplicación web, PostgreSQL, Redis y RabbitMQ. El backend aplicará las migraciones y los datos semilla de forma controlada al arrancar, de modo que los lugares de referencia y los usuarios iniciales queden disponibles sin pasos adicionales.

### Servicios

Una vez iniciados los contenedores, los servicios quedan disponibles en las siguientes direcciones:

| Servicio | Dirección | Notas |
|----------|-----------|-------|
| Aplicación web | http://localhost:4200 | Frontend Angular servido por Nginx |
| API | http://localhost:8080 | Endpoint de salud: `/health` |
| Documentación API (Scalar) | http://localhost:8080/scalar/v1 | Explorador interactivo de endpoints |
| PostgreSQL | localhost:5432 | Credenciales en `src/backend/.env` |
| Redis | localhost:6380 | Puerto de host 6380 para no colisionar con Redis local |
| RabbitMQ | localhost:5672 | Protocolo AMQP |
| RabbitMQ (panel) | http://localhost:15672 | Interfaz de administración |

Las credenciales de PostgreSQL, Redis y RabbitMQ se toman de `src/backend/.env` (los valores de ejemplo están en `src/backend/.env.example`).

### Usuarios semilla

El backend crea automáticamente dos usuarios al arrancar por primera vez:

| Rol | Correo | Contraseña |
|-----|--------|------------|
| Administrador | admin@eventosvivos.dev | `Admin123*` |
| Usuario | usuario@eventosvivos.dev | `Usuario123*` |

---

## Pruebas automatizadas

El proyecto sigue **desarrollo guiado por pruebas (TDD)**: ningún flujo de negocio se da por terminado sin sus pruebas, y el pipeline de CI las ejecuta con cobertura en cada pull request antes de permitir la integración.

### Backend

Tres proyectos de prueba con **xUnit**:

| Proyecto | Qué prueba |
|----------|------------|
| `EventosVivos.Domain.Tests` | Reglas de negocio, invariantes del dominio y máquinas de estado (entidades puras, sin infraestructura) |
| `EventosVivos.Application.Tests` | Casos de uso (handlers de Mediator), validaciones de FluentValidation y comportamiento de los pipeline behaviors |
| `EventosVivos.Api.Tests` | Endpoints de Minimal APIs de extremo a extremo, con base de datos real en contenedor (Testcontainers) |

### Frontend

Pruebas con **Vitest** y **Angular Testing Library**, organizadas junto a cada feature:

- Componentes: login, registro, listado de eventos, creación de eventos, listado de reservas, mis reservas, diálogo de reserva, home, reporte de ocupación.
- Stores de signals: autenticación, reservas, reportes.
- Interceptores: normalización de errores, envío de zona horaria.

---

## Integración y despliegue continuos

El pipeline de GitHub Actions tiene cuatro fases que se ejecutan en orden:

1. **CI — Backend y Frontend (en paralelo):** en cada pull request hacia `main` se comprueban el formato, se compila, se ejecutan las pruebas con cobertura y se envía el análisis a SonarCloud, cada proyecto a su propio espacio. El pull request queda en verde únicamente cuando ambos jobs terminan correctamente.

2. **Publicación de imágenes (CD):** al hacer push a `main`, si los dos jobs de CI pasan, se construyen y publican las imágenes Docker de backend y frontend en GitHub Container Registry (`ghcr.io`). Cada publicación calcula automáticamente la siguiente versión semántica (minor autoincremental), crea el tag en git (`vX.Y.Z`) y etiqueta las imágenes con ese tag y con `latest`.

3. **Despliegue al VPS (CD):** una vez publicadas las imágenes, el pipeline copia los archivos de despliegue al servidor vía SCP y reinicia la pila con `docker compose`, descargando las nuevas imágenes desde GHCR. El `.env` de producción ya vive en el servidor y no se sube en ningún momento.

### Secretos requeridos en GitHub

| Secreto | Descripción |
|---------|-------------|
| `SONAR_TOKEN` | Token de SonarCloud |
| `SONAR_ORGANIZATION` | Organización en SonarCloud |
| `SONAR_PROJECT_KEY_BACKEND` | Project key del proyecto backend |
| `SONAR_PROJECT_KEY_FRONTEND` | Project key del proyecto frontend |
| `SSH_HOST` | IP o dominio del VPS |
| `SSH_USER` | Usuario SSH del VPS |
| `SSH_KEY` | Clave privada SSH para autenticarse en el VPS |

Además, en SonarCloud se deben crear la organización y dos proyectos (uno por cada proyecto), vinculados a este repositorio. En GitHub, Settings → Actions → General → Workflow permissions debe tener habilitado "Read and write permissions" para que el pipeline pueda crear los tags de versión y publicar en GHCR.

## Documentación

- [`docs/ENUNCIADO.md`](./docs/ENUNCIADO.md) — enunciado del problema.
- [`docs/ARQUITECTURA-BACKEND.md`](./docs/ARQUITECTURA-BACKEND.md) — arquitectura del backend y su justificación.
- [`docs/ARQUITECTURA-FRONTEND.md`](./docs/ARQUITECTURA-FRONTEND.md) — arquitectura del frontend y su justificación.
- [`docs/skills/`](./docs/skills/README.md) — convenciones técnicas del proyecto.
