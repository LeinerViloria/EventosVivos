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

En el frontend se escriben pruebas unitarias para los servicios encargados del estado y de las llamadas a la API, para los pipes de internacionalización y de fechas, y para los guards e interceptores, todos verificados en aislamiento. Además se escriben pruebas de componente que validan el renderizado y el comportamiento de la interfaz con sus dependencias simuladas. El detalle del runner de pruebas y de la estructura concreta se definirá junto con la arquitectura del frontend.

## Reglas generales

Ningún flujo de negocio se considera terminado mientras no exista una prueba que lo cubra. Las pruebas se incluyen en el mismo pull request que el código que verifican, sin diferirse a un momento posterior. Finalmente, la cobertura y la calidad de las pruebas se revisan de forma automática en la integración continua, como se describe en el documento [quality-and-cicd](./quality-and-cicd.md).
