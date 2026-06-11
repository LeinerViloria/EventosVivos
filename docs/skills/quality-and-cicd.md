# Calidad e Integración Continua

La calidad del código no se confía a la disciplina individual, sino que se verifica de forma automática en la integración continua. Cada cambio que se incorpora al repositorio atraviesa una serie de comprobaciones, y cualquiera de ellas que falle detiene la incorporación.

## Verificaciones automáticas

La primera comprobación es el formato del código. Tal como se describe en el documento [code-conventions](./code-conventions.md), el backend ejecuta `dotnet format --verify-no-changes` y el frontend su linter y formateador equivalentes. Si el código no respeta las reglas definidas en el archivo `.editorconfig`, la construcción falla.

La segunda comprobación es la ejecución de las pruebas automatizadas. Se ejecutan las pruebas del backend, organizadas en los tres niveles descritos en el documento [tdd](./tdd.md), y las pruebas del frontend. Ningún cambio se incorpora si alguna prueba falla.

La tercera comprobación es el análisis estático de calidad con SonarQube. Esta herramienta revisa el código en busca de errores potenciales, vulnerabilidades, código duplicado y deuda técnica, y mide la cobertura de las pruebas. El proyecto define un quality gate, es decir, un conjunto de umbrales mínimos que el código debe cumplir para considerarse aceptable. Si el análisis no supera ese umbral, la construcción falla.

## Construcción y despliegue

El backend y el frontend se empaquetan como imágenes de Docker independientes, construidas en varias etapas para mantener las imágenes finales ligeras. El backend se compila sobre la imagen del SDK de .NET y se ejecuta sobre la imagen de runtime de ASP.NET. El frontend se compila y luego se sirve mediante un contenedor con Nginx.

Para el desarrollo local, un archivo de Docker Compose levanta el conjunto completo de servicios, que incluye la API, la aplicación web, PostgreSQL, Redis y RabbitMQ. Esto permite ejecutar todo el sistema en un entorno reproducible con un único comando.

El despliegue en la nube se realiza a partir de esas mismas imágenes, sobre un proveedor que admita contenedores y una instancia gestionada de PostgreSQL. La aplicación desplegada se ofrece como diferenciador, según lo valora el enunciado.

## Migraciones y datos semilla

El backend controla las migraciones de la base de datos mediante las migraciones versionadas de Entity Framework Core, sin recurrir a mecanismos como `EnsureCreated`. Asimismo, controla la información semilla de forma idempotente, de modo que datos de referencia como los venues queden disponibles de manera consistente en cada entorno. La estrategia concreta para aplicar las migraciones dentro del contenedor se define como parte de la configuración del despliegue.
