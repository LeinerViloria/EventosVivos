# Internacionalización y Enumeraciones

## Internacionalización

El frontend implementa internacionalización con soporte para un único idioma: el español de Colombia, identificado como `es-CO`. Aunque por ahora solo exista un idioma, la estructura queda preparada para incorporar otros en el futuro sin reescribir la aplicación.

La capa de internacionalización es responsable de traducir tres tipos de contenido: las etiquetas de la interfaz, los valores de las enumeraciones y los códigos de error descritos en el documento [error-handling](./error-handling.md).

## Enumeraciones numéricas

Las opciones cerradas del dominio se modelan como enumeraciones numéricas en ambos extremos de la aplicación. El contrato de la API siempre transporta el valor numérico, nunca el texto correspondiente.

| Concepto | Backend | Frontend |
|----------|---------|----------|
| Estado del evento | `enum : byte` | enumeración numérica de TypeScript |
| Estado de la reserva | `enum : byte` | enumeración numérica de TypeScript |
| Tipo de evento | `enum : byte` | enumeración numérica de TypeScript |
| Rol de usuario | `enum : byte` | enumeración numérica de TypeScript |

El flujo es sencillo. La API responde con el valor numérico, por ejemplo `{ "status": 1 }`. El frontend recibe ese número y utiliza la internacionalización para mostrar la etiqueta correspondiente, en este caso "Activo". En ningún momento se intercambian textos de dominio entre el backend y el frontend; el número es el contrato y la traducción es responsabilidad exclusiva del frontend.

## Beneficios de este enfoque

Este diseño ofrece un contrato estable e independiente del idioma. Cambiar un texto visible para el usuario no requiere modificar el backend, y se mantiene una consistencia total entre lo que se persiste en la base de datos, lo que viaja por la red y lo que se muestra en la pantalla.
