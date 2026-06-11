# Configuración y Secretos

La configuración sensible del proyecto se maneja mediante archivos de entorno, uno por proyecto. Cada proyecto, el backend y el frontend, tiene su propio archivo `.env`, que no se versiona, y un archivo `.env.example` que sí se versiona y actúa como plantilla. De esta manera, los secretos nunca llegan al repositorio, pero queda documentado qué variables necesita cada proyecto para funcionar.

## El `.env` del backend

El archivo `src/backend/.env` contiene los secretos reales del backend: las credenciales de PostgreSQL, de Redis y de RabbitMQ, y los secretos propios de la aplicación, como la clave de firma de los JSON Web Tokens. Estas credenciales de infraestructura se definen en un único lugar y se comparten con los servicios correspondientes a través de Docker Compose, de modo que el valor con el que se inicializa cada servicio y el valor con el que el backend se conecta a él sean siempre el mismo.

## El `.env` del frontend

El archivo `src/frontend/.env` contiene únicamente configuración no sensible, en concreto la URL de la API que el frontend consume. La razón es importante: un frontend es código que se ejecuta en el navegador, por lo que cualquier valor que se empaquete con él es público y no puede considerarse secreto. Por eso los secretos reales viven exclusivamente en el backend, y el frontend solo recibe la configuración que de todos modos sería visible para el usuario.

## Reglas

El archivo `.env` de cada proyecto está ignorado por git, junto con sus variantes, mientras que el `.env.example` se mantiene versionado con valores de marcador. Nunca se versiona un secreto real. Para poner en marcha el proyecto, se copia el `.env.example` a `.env` y se completan los valores.

Docker Compose inyecta estos archivos en los contenedores mediante la directiva `env_file`, y el backend lee su configuración desde las variables de entorno resultantes.
