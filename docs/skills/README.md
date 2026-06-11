# Skills — EventosVivos

Esta carpeta reúne las convenciones y competencias técnicas que se aplican de forma transversal a todo el proyecto, tanto en el backend como en el frontend. A diferencia de los documentos de arquitectura, que describen las decisiones estructurales de la solución, los documentos de skills describen la manera de trabajar: cómo se escribe el código, cómo se prueba, cómo se manejan los errores y cómo se asegura la calidad.

Cada skill es de cumplimiento obligatorio. Siempre que resulta posible, su cumplimiento se apoya en mecanismos automáticos, como la integración continua, el archivo `.editorconfig` y los quality gates, de manera que no dependa de la disciplina individual de cada persona.

Las decisiones estructurales del sistema se documentan aparte, en [`ARQUITECTURA-BACKEND.md`](../ARQUITECTURA-BACKEND.md) y en `ARQUITECTURA-FRONTEND.md`.

## Índice

| Skill | Descripción |
|-------|-------------|
| [development-workflow](./development-workflow.md) | Trabajo por ramas, avance secuencial e integración mediante pull request. |
| [code-conventions](./code-conventions.md) | Idioma del código, formato automático y convenciones de nombres. |
| [configuration-and-secrets](./configuration-and-secrets.md) | Archivos `.env` por proyecto, ignorados por git, con `.env.example` versionado. |
| [tdd](./tdd.md) | Desarrollo guiado por pruebas y estrategia de testing por capa. |
| [data-access](./data-access.md) | La base de datos resuelve los cálculos y búsquedas; LINQ sin SQL puro. |
| [search-endpoints](./search-endpoints.md) | Los selectores de campos relacionados se sirven en endpoints `/search`. |
| [database-modeling](./database-modeling.md) | Configuración con Fluent API e índices sin redundancia. |
| [error-handling](./error-handling.md) | Contrato de códigos de error compartido entre backend y frontend. |
| [form-validation](./form-validation.md) | Validación con Signal Forms; estructura en el cliente, reglas de negocio en el servidor. |
| [ui-components](./ui-components.md) | Tailwind y PrimeNG; componer con componentes reutilizables y presentar errores. |
| [i18n-and-enums](./i18n-and-enums.md) | Internacionalización en español de Colombia y enumeraciones numéricas. |
| [datetime-utc](./datetime-utc.md) | Manejo de fechas en UTC y zona horaria del cliente. |
| [security](./security.md) | Autenticación de dos tokens y manejo seguro de la información. |
| [quality-and-cicd](./quality-and-cicd.md) | Análisis de calidad, verificaciones automáticas e integración continua. |
