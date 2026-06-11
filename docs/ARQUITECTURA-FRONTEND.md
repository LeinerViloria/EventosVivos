# Arquitectura del Frontend — EventosVivos

> Documento de socialización. Igual que en el backend, las decisiones se toman de
> forma argumentada. El frontend usa **Angular 22.0.0**, una versión publicada el 3 de
> junio de 2026, por lo que este documento incluye un glosario de los términos nuevos
> que introduce. Las convenciones transversales viven en la carpeta
> [`skills/`](./skills/README.md) y el contrato con el backend, en
> [`ARQUITECTURA-BACKEND.md`](./ARQUITECTURA-BACKEND.md).

---

## 1. Glosario de términos de Angular

Angular 22 consolida un conjunto de conceptos que conviene aclarar antes de describir la arquitectura, porque varios de ellos son recientes.

Un **signal** es un contenedor de un valor que notifica automáticamente a quien lo usa cuando ese valor cambia. La interfaz se redibuja al cambiar los signals que consume, sin necesidad de mecanismos externos de detección.

La **era signal-first** es la dirección que adopta Angular 22, en la que los signals son el mecanismo primario para manejar el estado de la aplicación, por encima de otras herramientas que antes cumplían ese papel.

El **modo zoneless** significa que Angular ya no depende de la librería Zone.js para detectar cambios. En su lugar, los signals indican con precisión qué cambió y qué debe redibujarse. En Angular 22 este modo es el comportamiento por defecto y produce un renderizado más predecible y eficiente.

La estrategia **OnPush** es la forma en que un componente decide cuándo volver a dibujarse: solo cuando cambian sus entradas o los signals que utiliza, en lugar de hacerlo ante cualquier evento. En Angular 22 es el comportamiento por defecto y se complementa de forma natural con los signals.

La **Resource API** y, en particular, **`httpResource`**, son la forma declarativa de obtener datos del servidor. Se describe un recurso a partir de signals, el recurso se vuelve a consultar por sí solo cuando cambian sus entradas y expone su estado como signals de valor, error, indicador de carga y estado general. Maneja además las condiciones de carrera automáticamente, descartando los resultados de peticiones que quedaron obsoletas.

Los **Signal Forms** son la API estable de formularios basada en signals, con validación integrada, pensada para el modo zoneless y con menos código repetitivo que las formas anteriores.

Un **store basado en servicios** es un servicio de Angular que encapsula el estado de una feature mediante signals y expone los métodos para modificarlo. Es la unidad donde vive la lógica de datos de cada feature.

**Vitest** es el ejecutor de pruebas que Angular 22 adopta por defecto para proyectos nuevos, en reemplazo de Karma, que quedó descontinuado. Ofrece mayor velocidad y una mejor integración con las técnicas modernas de prueba.

---

## 2. Decisión arquitectónica

La arquitectura del frontend se organiza **por features**, y el estado se maneja con **signals nativos y stores basados en servicios**, sin recurrir a una librería de manejo de estado.

Esta decisión sigue la estrategia por niveles que recomienda el propio equipo de Angular y las guías de arquitectura de 2026: usar signals para el estado local y simple, un store basado en servicios para el estado compartido de una feature, y graduarse a una librería únicamente cuando la complejidad lo exija. Para un alcance de seis requerimientos funcionales, los stores por servicio son exactamente el punto adecuado. Incorporar una librería como NgRx sería una sobreingeniería que el tamaño del problema no justifica.

Si en el futuro la complejidad creciera, el camino de evolución natural sería `@ngrx/signals`, que hoy se alinea con los signals y permite reunir el estado, sus actualizadores y sus efectos en un único archivo por feature. Queda señalado como evolución posible, no como necesidad actual.

---

## 3. Estructura de carpetas

El frontend reside en `src/frontend/` dentro del monorepo. Para evitar la doble carpeta `src` que el CLI de Angular genera por defecto, se personaliza el proyecto mediante las propiedades `root` y `sourceRoot` de `angular.json`, de manera que `src/frontend` sea directamente la raíz del código. Así, archivos como `main.ts` e `index.html` residen en `src/frontend` y el código de la aplicación vive en `src/frontend/app`.

```
src/                          # Monorepo
  backend/                    # Proyectos .NET (ver ARQUITECTURA-BACKEND.md)
  frontend/                   # Proyecto Angular — raíz del código (sourceRoot)
    index.html
    main.ts
    app/
      core/                   # Interceptores (autorización, zona horaria, errores),
                              #   guards, servicio de Server-Sent Events y
                              #   configuración base de HttpClient
      shared/                 # Componentes de interfaz reutilizables, pipes
                              #   (traducción de enums, fechas locales), modelos y enums
      features/
        events/               # Listado, creación y detalle de eventos
        reservations/         # Reserva, confirmación de pago y cancelación
        reports/              # Reporte de ocupación
        auth/                 # Inicio de sesión y manejo de los dos tokens
      i18n/                   # Traducciones es-CO (etiquetas, enums y códigos de error)
```

La ruta de la aplicación desde la raíz del repositorio queda como `src/frontend/app`, sin la repetición de `src`. El único matiz es que esta disposición se aparta de la convención por defecto del CLI, que asume una carpeta `src`; en la práctica no genera inconvenientes, porque las herramientas leen la ruta configurada en `angular.json`.

Cada feature contiene su propio store basado en servicios, sus componentes y sus pruebas, de modo que el trabajo de una feature queda contenido en su carpeta.

---

## 4. Capa de datos

La capa de datos distingue entre lecturas y comandos, porque son operaciones de naturaleza distinta y se resuelven con herramientas distintas.

### Lecturas con `httpResource`

Las lecturas se modelan con `httpResource`, aprovechando su carácter reactivo. El caso más representativo es el listado de eventos con filtros (RF-02): los filtros viven como signals y el recurso se vuelve a consultar por sí solo cuando alguno cambia, sin orquestación manual y descartando respuestas obsoletas. El mismo enfoque aplica al detalle de un evento y al reporte de ocupación (RF-06). El componente no llama al API directamente, sino que lee los signals de valor, carga y error que expone el recurso.

Todo listado se pagina en el servidor, no solo el de eventos. Por eso, en las lecturas que devuelven listas, la página y el tamaño de página también viven como signals y forman parte de la petición del `httpResource`: al cambiar la página, el recurso se vuelve a consultar automáticamente. La respuesta incluye los metadatos de la paginación, como la página actual, el tamaño y el total de elementos, para que la interfaz pueda mostrar los controles de navegación.

### Comandos con `HttpClient`

Los comandos son las operaciones que cambian el estado del servidor y que dispara una acción del usuario: crear un evento (RF-01), reservar entradas (RF-03), confirmar un pago (RF-04) y cancelar una reserva (RF-05). No son recursos reactivos, sino llamadas imperativas, y se realizan con `HttpClient` desde el store de la feature. Forzarlos dentro de `httpResource` sería usar la herramienta para algo que no le corresponde.

### Dónde vive la lógica de datos

Tanto los recursos de lectura como los comandos viven en el store basado en servicios de cada feature. El store expone los signals derivados de los recursos y los métodos para los comandos, y los componentes solo consumen esos signals. Así, la lógica de datos queda fuera de los componentes y resulta fácil de probar con Vitest.

### Interceptores transversales

Tres preocupaciones transversales se resuelven en un único lugar, mediante interceptores de `HttpClient`. Como `httpResource` utiliza `HttpClient` por debajo, los interceptores aplican por igual a lecturas y comandos. El primer interceptor inyecta el token de identidad en la cabecera de autorización. El segundo inyecta la zona horaria del cliente en su cabecera, según el manejo de fechas descrito en los skills. El tercero normaliza los errores.

### Normalización de errores

Un interceptor captura las respuestas de error del backend, interpreta el `ProblemDetails` y lo convierte en un error tipado de la aplicación, con la forma `{ errorCode, errorKind, params }`. De esta manera, ni los stores ni los componentes manipulan respuestas HTTP crudas: siempre reciben un error con código, que traducen mediante internacionalización usando los parámetros para interpolar, tal como define el contrato de errores. El signal de error de cada `httpResource` contiene ya ese error normalizado.

### Revalidación y eventos en vivo

Cuando un comando termina con éxito, el store vuelve a cargar el recurso de lectura afectado, de modo que la vista refleje el nuevo estado. Para las notificaciones en tiempo real, el servicio de Server-Sent Events alimenta un signal de disponibilidad; cuando llega una liberación de entradas, ese signal se actualiza y, según el caso, refresca el recurso correspondiente para que la vista muestre la disponibilidad actualizada sin intervención del usuario.

---

## 5. Formularios con Signal Forms

Los formularios se construyen con Signal Forms, la API estable de Angular 22 para formularios basados en signals. La función `form()` crea un árbol de campos reactivo que refleja el modelo del formulario, y cada campo expone su estado como signals: si es válido o inválido, su lista de errores, si tiene una validación asíncrona en curso y si el usuario ya lo tocó o lo modificó. El estado se propaga hacia arriba, de modo que el formulario completo es inválido si alguno de sus campos lo es. La directiva `FormRoot` maneja el envío: marca todos los campos como tocados para revelar los errores y, solo si el formulario es válido, ejecuta la acción de envío.

### Validación del formulario de creación de evento (RF-01)

El formulario de creación de evento se apoya en los tres tipos de validación que ofrece Signal Forms. Los validadores integrados cubren los campos obligatorios y las longitudes del título y la descripción, así como los mínimos del precio y la capacidad. Un validador personalizado verifica que la fecha de inicio sea futura, comparándola con la hora actual. Una validación cruzada verifica que la fecha de fin sea posterior a la de inicio, leyendo el valor del otro campo. La capacidad menor o igual a la del venue también se valida en el cliente, porque la capacidad del venue ya está disponible al haberse cargado los venues, lo que ofrece retroalimentación inmediata.

### Los errores de validación usan el mismo contrato de códigos

Cada error de Signal Forms tiene un identificador de tipo, su `kind`. Ese `kind` se usa como código de error, alineado con el mismo catálogo que comparten backend y frontend, y se traduce en la plantilla mediante internacionalización, interpolando los parámetros. De esta manera, los errores de validación del cliente se muestran con exactamente el mismo mecanismo que los errores del servidor: un código que la internacionalización convierte en texto en español de Colombia. No existen dos sistemas de mensajes, sino uno solo, tanto para los validadores integrados como para los personalizados.

### División de responsabilidades entre cliente y servidor

El cliente valida la estructura de la entrada y las comprobaciones cruzadas de bajo costo, con el fin de dar retroalimentación inmediata. Las reglas de negocio, RN01 a RN07, son responsabilidad del servidor y constituyen su fuente de verdad. Cuando una de ellas no se cumple, el servidor responde con un `ProblemDetails` que incluye su código de error, el cual el frontend ya sabe traducir y mostrar. No se reimplementan esas reglas como validadores asíncronos en el cliente, porque duplicar la lógica abriría la puerta a discrepancias entre cliente y servidor. Por defecto, las reglas de negocio se resuelven en el momento del envío, a través de la respuesta de error del comando.

Signal Forms admite además la validación por esquema con librerías como Zod o Valibot. No se utiliza en este proyecto, porque los validadores nativos y personalizados cubren RF-01 sin añadir una dependencia adicional y mantienen la validación junto a la definición del formulario.

---

## 6. Internacionalización y enumeraciones

### Internacionalización en tiempo de ejecución con Transloco

La internacionalización se resuelve en tiempo de ejecución con Transloco, y no con el enfoque de compilación de `@angular/localize`. La razón es concreta: el frontend no solo traduce textos fijos de las plantillas, sino claves dinámicas que se conocen en tiempo de ejecución, como los códigos de error que llegan en las respuestas del backend y los valores numéricos de los enums. Una librería de tiempo de ejecución traduce esas claves de forma natural y con interpolación de parámetros, mientras que el enfoque de compilación, pensado para textos estáticos marcados en la plantilla, se vuelve incómodo para traducir una clave que solo se conoce al recibir la respuesta. La versión de Transloco se fijará a la compatible con Angular 22 al momento de implementar.

### Estructura del archivo de traducciones

Las traducciones residen en un único archivo `es-CO.json`, organizado en tres secciones que separan las clases de texto que maneja la aplicación.

```json
{
  "labels": { "events.title": "Eventos", "reservations.confirm": "Confirmar pago" },
  "enums": {
    "eventStatus": { "1": "Activo", "2": "Cancelado", "3": "Completado" },
    "eventType":   { "1": "Conferencia", "2": "Taller", "3": "Concierto" },
    "reservationStatus": { "1": "Pendiente de pago", "2": "Confirmada", "3": "Cancelada", "4": "Perdida" }
  },
  "errors": {
    "RESERVATION_ALREADY_CONFIRMED": "La reserva ya está confirmada",
    "MAX_TICKETS_PER_TRANSACTION_EXCEEDED": "Solo puedes reservar máximo {{max}} entradas"
  }
}
```

La sección de etiquetas contiene los textos de la interfaz. La sección de enums se organiza por tipo de enum y, dentro de cada uno, por el valor numérico, que es exactamente lo que viaja en el contrato. La sección de errores usa el código de error como clave, con marcadores para interpolar los parámetros.

### Traducción de enumeraciones mediante un pipe puro

Para traducir una enumeración se usa un pipe puro que recibe el valor numérico y el tipo de enum, y devuelve la etiqueta. En la plantilla se utiliza de la forma `{{ event.status | enumLabel:'eventStatus' }}`, y el pipe busca en la sección de enums la entrada correspondiente al número recibido. Al ser un pipe puro, no recalcula salvo que cambie el valor, lo que resulta eficiente. Esto cierra el círculo con la capa de datos: el número se conserva intacto hasta la presentación, y solo aquí se convierte en texto.

### Traducción de los códigos de error

Los códigos de error se traducen con el mismo archivo, usando la sección de errores y la interpolación de parámetros. Como los errores ya se normalizan a la forma `{ errorCode, params }`, tanto los que devuelve el servidor como los de validación de Signal Forms, la presentación toma el código, busca su clave en la sección de errores e interpola los parámetros. Es un único camino de traducción para ambos orígenes de error.

### Formato de fechas, números y moneda

Se registra el locale `es-CO` en la aplicación, de modo que los pipes de fecha, número y moneda usen el formato colombiano. Esto se conecta con el manejo de fechas: el backend entrega las fechas en tiempo universal y la presentación las muestra en la hora local con el formato de `es-CO`. El precio del evento se formatea también según ese locale.

---

## 7. Servicio de Server-Sent Events

En la carpeta `core` vive un servicio de Server-Sent Events que abre la conexión con el endpoint de stream del backend mediante el `EventSource` nativo del navegador. El servicio no expone eventos crudos: los interpreta y los publica como signals tipados. Cada mensaje del stream tiene un tipo y un cuerpo en formato JSON; el servicio identifica el tipo, por ejemplo una liberación de entradas, y actualiza el signal correspondiente con los datos recibidos, que respetan el mismo contrato numérico del resto del sistema.

La integración con el resto de la aplicación es la prevista en la capa de datos. Los stores de las features observan esos signals y reaccionan: cuando llega una liberación de entradas, el store recarga el recurso de lectura afectado para que la vista muestre la nueva disponibilidad. El modo zoneless juega a favor, porque al ser los signals los que disparan el redibujado, actualizar uno desde la devolución de llamada del `EventSource` refresca la interfaz sin mecanismos adicionales.

### Autenticación del stream

Como el `EventSource` no permite enviar la cabecera de autorización, el token de identidad viaja por la cadena de consulta de la URL del stream. Se descartó el uso de una cookie httpOnly por considerarse sobreingeniería para el alcance de un producto mínimo viable, ya que implicaría configuración adicional de cookies y de CORS.

Esta decisión es aceptable porque el token aporta dos filtros de seguridad. El primero es su tiempo de vida corto, que limita la ventana en la que un token expuesto sería útil. El segundo es su firma, que impide que un token manipulado sea aceptado. A esto se suma que es un token solo de identidad, de tamaño reducido, y que la sesión es revocable contra Redis. El costo conocido es que el token queda visible en la URL y puede registrarse en logs; se asume de forma consciente para el alcance actual, dejando la cookie httpOnly como evolución de seguridad si el proyecto creciera.

### Reconexión y ciclo de vida

El `EventSource` reintenta la conexión automáticamente cuando se cae, comportamiento que se aprovecha para los cortes de red normales. Sin embargo, ante un error de autenticación, como un token expirado, reintentaría en vano; por eso el servicio maneja ese caso explícitamente, cerrando la conexión y volviéndola a abrir una vez renovado el token.

La conexión se abre una vez que el usuario ha iniciado sesión y se cierra al cerrar sesión. Se mantiene una única conexión que transporta los eventos relevantes y que el servicio distribuye por tipo, en lugar de abrir varias, lo que es más simple y respeta los límites de conexiones del navegador.

---

## 8. Estrategia de pruebas

Las pruebas del frontend se ejecutan con Vitest, el ejecutor que Angular 22 adopta por defecto. Para las pruebas de componente se utiliza además Angular Testing Library, una capa de utilidades que se apoya en el `TestBed` de Angular y que orienta las pruebas hacia el comportamiento observable por el usuario. Conviene tener presente que estas piezas se apilan y no compiten: Vitest ejecuta todas las pruebas, el `TestBed` configura los componentes y Angular Testing Library ayuda a renderizarlos y consultarlos. La versión compatible con Angular 22 se fijará al implementar.

### Dónde se concentra el esfuerzo

La lógica del frontend vive sobre todo en los stores basados en servicios, de modo que ahí se concentra el grueso de las pruebas: el manejo del estado, la orquestación de los comandos, los signals derivados y el manejo de errores, simulando la capa HTTP.

Junto a ellas se prueban las unidades puras, rápidas y aisladas: los validadores personalizados de Signal Forms, el pipe de traducción de enumeraciones, los interceptores de autorización, de zona horaria y de normalización de errores, y la lógica de mapeo de errores.

Las pruebas de componente verifican el comportamiento de la interfaz con sus dependencias simuladas, por ejemplo que un formulario bloquee el envío cuando es inválido, que los errores aparezcan al tocar un campo o que una lista muestre lo que entrega su store. Se enfocan en lo que el usuario ve y hace, no en los detalles internos.

Por último, el servicio de Server-Sent Events se prueba simulando el `EventSource`, para verificar que un evento entrante actualiza el signal correspondiente y que la reconexión distingue un corte de red de un error de autenticación.

### Simulación de dependencias

Para las llamadas HTTP, tanto de los recursos de lectura como de los comandos, se usa el backend de pruebas de `HttpClient`, que permite controlar las respuestas y verificar las peticiones sin tocar la red. El `EventSource` se reemplaza por un doble en las pruebas. Transloco se configura con un stub de traducciones, de modo que las pruebas no dependan del archivo real de idioma.

El modo zoneless facilita las pruebas: como los signals son los que disparan el redibujado, las pruebas leen el valor de los signals después de ejecutar una acción, sin necesidad de los mecanismos de zona que antes complicaban las pruebas asíncronas. Lo asíncrono, como un `httpResource` o un comando, se resuelve esperando en la prueba.

### Cobertura

Vitest mide la cobertura, que se integra en el análisis de SonarQube y queda sujeta al quality gate definido en la integración continua. Así, la cobertura y la calidad de las pruebas del frontend se verifican de forma automática, igual que en el backend.

---

## 9. Interfaz de usuario

La base de estilos es Tailwind, y los componentes provienen de PrimeNG, la librería de componentes de Angular más completa, con más de ochenta componentes de licencia MIT ya funcionales. PrimeNG cubre de entrada lo que el proyecto necesita: tablas con paginación, que encajan con la paginación resuelta en el servidor, formularios, diálogos y componentes de mensajes y notificaciones para los errores. La integración con Tailwind se realiza con el plugin oficial de PrimeNG, que funciona en modo con estilos y sin estilos, y la tematización reciente de PrimeNG se basa en variables y tokens, lo que evita los conflictos de estilo que solían darse entre Tailwind y otras librerías. La versión compatible con Angular 22 se fija al implementar.

Se consideró SpartanNG, una opción pensada para Tailwind desde su base que entrega control total de los componentes al copiarlos dentro del proyecto. Se descartó para este alcance porque implica más trabajo de ensamblaje y no encaja tan bien con el objetivo de partir de una librería que ya provea componentes funcionales listos.

El principio de construcción de la interfaz es componer con componentes y no escribir HTML crudo. La interfaz se arma principalmente con los componentes de PrimeNG. Cuando un caso necesita personalización o se repite en varias vistas, se extrae a un componente reutilizable propio, ubicado en la carpeta `shared`, que normalmente envuelve uno o varios componentes de PrimeNG aplicando las convenciones del proyecto. Así, la lógica de presentación común queda en un solo lugar y las vistas se construyen ensamblando componentes.

---

## 10. Presentación de errores

La presentación de errores aprovecha el error tipado `{ errorCode, errorKind, params }` y el campo `errorKind` para decidir cómo mostrar cada error. En todos los casos, el texto se obtiene traduciendo el código con Transloco e interpolando los parámetros, de modo que se mantiene el único camino de traducción definido. La librería ya ofrece los componentes necesarios, por lo que no se incorpora ninguna dependencia adicional para esto.

Los errores de validación, los de Signal Forms, se muestran en línea junto al campo correspondiente, con el componente de mensaje de PrimeNG. Es el lugar natural, porque el usuario necesita ver qué campo corregir mientras llena el formulario.

Los errores de negocio, los que devuelve el servidor cuando una regla RN01 a RN07 no se cumple, se muestran como un toast, una notificación temporal en una esquina, con el componente de toast de PrimeNG. Son el resultado de una acción, como confirmar un pago o reservar, y un toast los comunica sin interrumpir el flujo.

Los errores generales, los inesperados del sistema, se muestran también como un toast, pero con un mensaje genérico, sin filtrar detalles internos, en coherencia con la decisión de seguridad del backend.
