# Seguridad

## Autenticación con dos tokens

La autenticación se basa en JSON Web Tokens. El sistema maneja dos roles, administrador y usuario, representados como una enumeración de tipo byte. Cuando una persona inicia sesión, el backend emite dos tokens con propósitos distintos.

El primero es el token de identidad. Viaja en cada petición al backend, dentro del header de autorización, y sirve para identificar a quien realiza la solicitud. Además del identificador del usuario y su rol, transporta el identificador de la sesión.

El segundo es el token de permisos. Reside en el frontend y su única función es decidir qué elementos de la interfaz se muestran y cuáles se ocultan, como menús, botones o vistas completas.

## La interfaz no es la fuente de autorización

El token de permisos mejora la experiencia del usuario, pero no constituye un control de seguridad. La autorización real siempre se aplica en el backend, que valida los permisos contra Redis en cada petición. De este modo, aunque alguien manipule el token de permisos en el cliente para revelar opciones que no le corresponden, el backend rechazará cualquier acción no autorizada.

## Modelo de permisos en Redis: dos niveles

La validación de permisos se apoya en dos estructuras almacenadas en Redis, que se complementan.

El primer nivel es el catálogo que asocia cada rol con sus permisos. Define qué puede hacer cada rol y, al ser único para todo el sistema, cualquier cambio en los permisos de un rol se refleja de inmediato para todos los usuarios. Por su tamaño reducido se puede sembrar como configuración.

El segundo nivel es una clave por sesión, que asocia el identificador de la sesión con los datos del usuario, su rol y el momento de emisión, y que tiene un tiempo de vida definido. Esta clave es la que permite revocar el acceso de un usuario concreto de forma inmediata, simplemente eliminándola, y es la que vincula el token de identidad con una sesión viva.

En cada petición el backend realiza dos comprobaciones. Primero verifica que la sesión siga existiendo en Redis, lo que funciona como control de revocación. A continuación resuelve los permisos efectivos del usuario a partir de su rol, consultando el catálogo del primer nivel. Gracias a esta combinación, los cambios en los permisos de un rol se aplican al instante para todos y, al mismo tiempo, es posible revocar el acceso de un usuario individual sin esperar a que su token caduque.

El mapeo entre cada endpoint y el permiso que requiere se aplica mediante políticas de autorización en los grupos de rutas. Por ejemplo, la confirmación del pago de una reserva, descrita en el requerimiento RF-04, queda restringida al rol de administrador.

## Manejo seguro de la información

Las excepciones no controladas se devuelven al cliente como un error 500 genérico, sin exponer trazas de pila ni detalles internos del sistema. Esta decisión, además de ofrecer respuestas uniformes, evita filtrar información que podría facilitar un ataque.

En el canal de notificaciones en tiempo real mediante Server-Sent Events existe una consideración particular. La API `EventSource` del navegador no permite enviar el header de autorización, por lo que el token de identidad debe transmitirse por medio de la cadena de consulta de la URL o de una cookie. Esta característica se tiene en cuenta al asegurar dicho endpoint.
