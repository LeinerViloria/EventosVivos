# Flujo de Desarrollo

El desarrollo del proyecto avanza feature por feature, de manera secuencial y con un control de calidad obligatorio antes de cada integración. El objetivo es que la rama principal se mantenga siempre en un estado estable y verificado.

## Una rama por feature

Cada feature se trabaja en su propia rama, creada a partir de `main`. Toda la implementación de ese feature, incluido su código y sus pruebas, ocurre dentro de esa rama, sin mezclarse con el trabajo de otros features.

Para mantener la coherencia con la convención de idioma del proyecto, los nombres de las ramas se escriben en inglés y describen el feature al que corresponden, por ejemplo `feature/reservations-create-reservation`.

## Avance secuencial

No se comienza a trabajar en el siguiente feature hasta que el feature actual se haya integrado a `main` a través de su pull request. Esta regla evita acumular trabajo sin integrar, mantiene el alcance de cada cambio acotado y facilita la revisión, porque cada pull request contiene un único feature completo y verificado.

## Integración mediante pull request

La integración a `main` siempre se realiza mediante un pull request. Al abrirlo se ejecutan de forma automática los pipelines de calidad descritos en el documento [quality-and-cicd](./quality-and-cicd.md), que verifican el formato del código, ejecutan las pruebas automatizadas y realizan el análisis estático con SonarQube.

Como parte de esa ejecución, el pull request entrega de inmediato el reporte de SonarQube, de modo que la calidad del cambio queda visible antes de aprobar la integración. Un pull request solo se fusiona a `main` cuando todas las verificaciones pasan y su quality gate se cumple. Si alguna verificación falla, el cambio se corrige en la misma rama hasta que el pipeline quede en verde.

## Entrega ejecutable para pruebas

La referencia para poder probar el sistema ejecutándolo es un frontend y un backend integrados, funcionando en conjunto. No se considera suficiente que cada parte funcione por separado: el criterio es poder levantar la aplicación completa y recorrer los flujos de negocio de extremo a extremo, desde la interfaz web hasta la base de datos.

En la práctica, a medida que los features se integran a `main`, la aplicación debe poder iniciarse por completo con el entorno de Docker Compose descrito en el documento [quality-and-cicd](./quality-and-cicd.md), levantando todos los servicios que la componen. Recorrer ese camino feliz con el frontend y el backend integrados es lo que sirve de referencia tanto para las pruebas manuales como para la demostración de la aplicación desplegada en la nube.
