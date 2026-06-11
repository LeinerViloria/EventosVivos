# Desarrollo Guiado por Pruebas (TDD)

El desarrollo guiado por pruebas es obligatorio tanto en el backend como en el frontend. La arquitectura de la solución se diseñó precisamente para ser fácil de probar: el dominio es puro y la infraestructura se encuentra detrás de abstracciones, de modo que cada pieza puede verificarse en aislamiento.

## El ciclo de trabajo

El desarrollo sigue el ciclo clásico de tres pasos. Primero se escribe una prueba que falla y que describe el comportamiento esperado. A continuación se escribe el mínimo código necesario para que esa prueba pase. Por último se refactoriza el código para mejorarlo, con la seguridad de que las pruebas existentes detectarán cualquier regresión.

## Estrategia de pruebas en el backend

Las pruebas del backend se organizan en tres niveles, cada uno con un alcance y una velocidad distintos.

| Proyecto | Alcance | Velocidad |
|----------|---------|-----------|
| `EventosVivos.Domain.Tests` | Verifica las invariantes de negocio (RN01 a RN07) y las máquinas de estado, sin depender de infraestructura. Concentra la mayor parte de las pruebas. | Muy rápidas |
| `EventosVivos.Application.Tests` | Verifica los casos de uso (handlers) utilizando repositorios en memoria o dobles de prueba. | Rápidas |
| `EventosVivos.Api.Tests` | Verifica el sistema de forma integrada, mediante `WebApplicationFactory` y una base de datos PostgreSQL real provista por Testcontainers. | Más lentas |

Tal como lo señala el enunciado, los casos borde y las validaciones se prueban con el mismo rigor que los flujos principales.

## Estrategia de pruebas en el frontend

El ejecutor de pruebas es Vitest, el que Angular 22 adopta por defecto. Para las pruebas de componente se utiliza además Angular Testing Library sobre el `TestBed`, que orienta las pruebas hacia el comportamiento observable por el usuario. Estas piezas se apilan y no compiten: Vitest ejecuta todas las pruebas, el `TestBed` configura los componentes y Angular Testing Library ayuda a renderizarlos y consultarlos.

El grueso de las pruebas se concentra en los stores basados en servicios, donde vive la lógica: el manejo del estado, la orquestación de los comandos, los signals derivados y el manejo de errores. Junto a ellos se prueban las unidades puras, como los validadores de Signal Forms, el pipe de traducción de enumeraciones y los interceptores de autorización, zona horaria y normalización de errores. Las pruebas de componente verifican el comportamiento de la interfaz, y el servicio de Server-Sent Events se prueba simulando el `EventSource`.

Para simular las dependencias se usa el backend de pruebas de `HttpClient` en las llamadas a la API, un doble del `EventSource` en el servicio de eventos y un stub de traducciones para Transloco. El modo zoneless simplifica las pruebas, porque basta con leer el valor de los signals después de ejecutar una acción. La cobertura la mide Vitest y se integra en el quality gate de SonarQube.

## Reglas generales

Ningún flujo de negocio se considera terminado mientras no exista una prueba que lo cubra. Las pruebas se incluyen en el mismo pull request que el código que verifican, sin diferirse a un momento posterior. Finalmente, la cobertura y la calidad de las pruebas se revisan de forma automática en la integración continua, como se describe en el documento [quality-and-cicd](./quality-and-cicd.md).
