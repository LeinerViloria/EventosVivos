# Convenciones de Código

## Idioma del código

Todo el código del proyecto se escribe en inglés. Esta regla abarca los nombres de carpetas, clases, métodos, variables, enumeraciones y endpoints, así como los mensajes de los commits. La única excepción es la documentación, que se redacta en español y reside en la carpeta `docs/`.

Para mantener la coherencia al traducir al inglés los conceptos del dominio descritos en el enunciado, el equipo adopta el siguiente mapa de términos:

| Español | Inglés |
|---------|--------|
| reserva | `Reservation` |
| evento | `Event` |
| venue | `Venue` |
| entrada | `Ticket` |
| pago | `Payment` |
| comprador | `Buyer` |

## Formato del código

Al finalizar cada desarrollo se ejecuta el formateador de código. Para que esta práctica no dependa de que cada persona recuerde aplicarla, se sostiene sobre tres mecanismos complementarios.

En primer lugar, un archivo `.editorconfig` ubicado en la raíz del repositorio actúa como fuente única de verdad de las reglas de estilo y formato. Tanto la herramienta `dotnet format` como el editor de código y el linter del frontend obtienen sus reglas de ese archivo.

En segundo lugar, la integración continua incluye una verificación que falla la construcción cuando el código no está correctamente formateado. En el backend esa verificación se realiza con el comando `dotnet format --verify-no-changes`, y en el frontend con el linter y el formateador equivalentes de Angular. De esta manera, la regla de mantener el código formateado se garantiza de forma objetiva, sin importar quién realice el aporte.

En tercer lugar, y de manera opcional, un hook de pre-commit puede ejecutar el formateo antes de cada commit para detectar cualquier desviación lo antes posible.

```bash
# Formatear el código del backend
dotnet format

# Verificar el formato sin modificar archivos (lo que ejecuta la integración continua)
dotnet format --verify-no-changes
```

## Convenciones de nombres

En el backend se siguen las convenciones estándar de .NET. Los tipos y los métodos utilizan `PascalCase`, las variables locales `camelCase`, los campos privados `_camelCase` y las interfaces se prefijan con la letra `I`.

En el frontend se siguen las convenciones estándar de Angular y TypeScript. Las clases y los componentes utilizan `PascalCase`, las variables y los métodos `camelCase`, y los nombres de archivo y los selectores `kebab-case`.

## Gestor de paquetes del frontend

El gestor de paquetes del frontend es **pnpm**. No se usa npm. Esta regla abarca la instalación de dependencias, la ejecución de scripts y la generación del proyecto, y se refleja también en el Dockerfile del frontend y en el pipeline de integración continua, que utilizan pnpm. pnpm se habilita mediante corepack, que viene incluido con Node, de modo que no es necesario instalar nada con npm.
