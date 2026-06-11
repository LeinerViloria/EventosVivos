# Manejo de Errores

El backend y el frontend comparten un contrato de errores basado en códigos. El backend nunca envía el texto que verá el usuario final; en su lugar, envía un código de error estable, y el frontend se encarga de traducirlo mediante internacionalización al español de Colombia. De esta manera, el idioma y la redacción de los mensajes son responsabilidad exclusiva del frontend.

## Dos tipos de error en el backend

El backend distingue entre dos clases de error, porque no son lo mismo y se tratan de forma diferente.

Los fallos de negocio esperados se representan con el patrón `Result<T>`, sin recurrir a excepciones. Son resultados normales del dominio, como intentar confirmar una reserva ya confirmada, reservar cuando no hay entradas disponibles o reservar un evento que comienza en menos de una hora.

Las fallas excepcionales, en cambio, sí se representan con excepciones. Corresponden a situaciones que no deberían ocurrir durante la operación normal, como una base de datos caída o un Redis inalcanzable. Estas excepciones se propagan hacia arriba y las captura un único middleware.

## El contrato de error

Todas las respuestas de error se traducen al formato ProblemDetails, definido por el RFC 7807, y siempre incluyen un código de error estable que el frontend puede interpretar.

```jsonc
409 {
  "type": "...", "title": "...", "status": 409,
  "errorCode": "RESERVATION_ALREADY_CONFIRMED",
  "errorKind": "business",
  "params": { }
}
```

El campo `errorCode` es de tipo `string` y no una enumeración, porque se trata de un identificador de contrato que se corresponde de forma directa con una clave de internacionalización. El campo `errorKind` indica si el error es de negocio o general. El campo `params` transporta los valores dinámicos que el frontend necesita para construir el mensaje final. Por ejemplo, el código `MAX_TICKETS_PER_TRANSACTION_EXCEEDED` viaja con el parámetro `{ "max": 5 }`, y el código `NOT_ENOUGH_TICKETS` viaja con `{ "requested": 10, "available": 3 }`.

Los errores de validación se devuelven con el código de estado 422 y contienen una lista de objetos con la forma `{ field, errorCode, params }`, de modo que el frontend pueda señalar cada campo afectado. Las excepciones no controladas se devuelven como un error 500 genérico, sin filtrar detalles internos del sistema, según se explica en el documento [security](./security.md).

## El catálogo de códigos como contrato compartido

El conjunto de códigos de error es un contrato que ambos lados deben respetar. En el backend existe como constantes, y en el frontend como claves dentro del archivo de traducciones `es-CO.json`. El frontend utiliza esas claves para construir el mensaje, interpolando los parámetros recibidos:

```json
{ "MAX_TICKETS_PER_TRANSACTION_EXCEEDED": "Solo puedes reservar máximo {{max}} entradas" }
```

Toda incorporación de un nuevo código de error debe reflejarse en ambos lados dentro del mismo pull request, para que el contrato nunca quede incompleto.
