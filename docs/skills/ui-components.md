# Componentes de Interfaz

## Tailwind y PrimeNG

La base de estilos del frontend es Tailwind, y los componentes provienen de PrimeNG, integrados mediante el plugin oficial que PrimeNG ofrece para Tailwind. PrimeNG aporta un amplio conjunto de componentes ya funcionales, como tablas con paginación, formularios, diálogos, mensajes y notificaciones, lo que permite construir la interfaz sin partir de cero. La versión compatible con Angular 22 se fija al implementar.

## No se escribe HTML crudo

El principio central es componer la interfaz con componentes, no marcar HTML a mano. Las vistas se arman ensamblando los componentes de PrimeNG. Cuando un caso necesita personalización, o cuando un mismo elemento se repite en varias vistas, se extrae a un componente reutilizable propio, ubicado en la carpeta `shared`. Ese componente propio normalmente envuelve uno o varios componentes de PrimeNG y aplica las convenciones del proyecto, de modo que la lógica de presentación común quede en un único lugar y las vistas se mantengan declarativas y consistentes.

## Presentación de errores

La presentación de errores usa los componentes que ya ofrece PrimeNG, sin añadir dependencias, y se decide según el `errorKind` del error tipado. En todos los casos el texto se obtiene traduciendo el código de error con internacionalización, según el contrato descrito en el documento [error-handling](./error-handling.md).

Los errores de validación se muestran en línea, junto al campo correspondiente, con el componente de mensaje. Los errores de negocio, que devuelve el servidor cuando una regla no se cumple, se muestran como un toast, porque son el resultado de una acción y conviene comunicarlos sin interrumpir el flujo. Los errores generales del sistema se muestran también como un toast, pero con un mensaje genérico que no filtra detalles internos.
