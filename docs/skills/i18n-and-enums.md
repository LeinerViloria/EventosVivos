# Internacionalización y Enumeraciones

## Internacionalización

El frontend implementa internacionalización con soporte para un único idioma: el español de Colombia, identificado como `es-CO`. Aunque por ahora solo exista un idioma, la estructura queda preparada para incorporar otros en el futuro sin reescribir la aplicación.

Se utiliza Transloco, una librería de internacionalización en tiempo de ejecución. Esta elección responde a que el frontend traduce claves que solo se conocen en ejecución, como los códigos de error que llegan del backend y los valores numéricos de las enumeraciones; el enfoque de compilación de `@angular/localize`, pensado para textos estáticos de las plantillas, no resuelve eso con naturalidad.

La capa de internacionalización es responsable de traducir las etiquetas de la interfaz, las etiquetas de los campos de formulario, los valores de las enumeraciones y los códigos de error descritos en el documento [error-handling](./error-handling.md). Las traducciones residen en un único archivo `es-CO.json` organizado en secciones: `labels` para las etiquetas generales de la interfaz, `field` para las etiquetas de los campos de formulario, `enums` para los valores de las enumeraciones y `errors` para los códigos de error.

Las etiquetas de los **campos de formulario** se identifican con el segmento `field` en su clave, por ejemplo `field.event.title`. Esto distingue las etiquetas de campos del resto de textos de la interfaz y mantiene una convención uniforme para todos los formularios. Adicionalmente, se registra el locale `es-CO` para que los formatos de fecha, número y moneda sigan la convención colombiana.

## Enumeraciones numéricas

Las opciones cerradas del dominio se modelan como enumeraciones numéricas en ambos extremos de la aplicación. El contrato de la API siempre transporta el valor numérico, nunca el texto correspondiente.

| Concepto | Backend | Frontend |
|----------|---------|----------|
| Estado del evento | `enum : byte` | enumeración numérica de TypeScript |
| Estado de la reserva | `enum : byte` | enumeración numérica de TypeScript |
| Tipo de evento | `enum : byte` | enumeración numérica de TypeScript |
| Rol de usuario | `enum : byte` | enumeración numérica de TypeScript |

El flujo es sencillo. La API responde con el valor numérico, por ejemplo `{ "status": 1 }`. El frontend recibe ese número y utiliza la internacionalización para mostrar la etiqueta correspondiente, en este caso "Activo". En ningún momento se intercambian textos de dominio entre el backend y el frontend; el número es el contrato y la traducción es responsabilidad exclusiva del frontend.

En el frontend, la traducción de una enumeración se realiza con un pipe puro que recibe el valor numérico y el tipo de enum, y devuelve la etiqueta. La sección de enumeraciones del archivo de traducciones se organiza por tipo de enum y, dentro de cada uno, por valor numérico, que es justamente lo que viaja en el contrato.

## Beneficios de este enfoque

Este diseño ofrece un contrato estable e independiente del idioma. Cambiar un texto visible para el usuario no requiere modificar el backend, y se mantiene una consistencia total entre lo que se persiste en la base de datos, lo que viaja por la red y lo que se muestra en la pantalla.
