# Validación de Formularios

Los formularios del frontend se construyen con Signal Forms, la API de formularios basada en signals de Angular. Esta skill recoge las convenciones de validación que deben seguirse de forma consistente en cualquier formulario del proyecto, más allá de un caso concreto.

## El cliente valida la estructura; el servidor, las reglas de negocio

La validación se reparte entre las dos capas según su naturaleza, y esa división se respeta siempre.

El cliente valida la estructura de la entrada y las comprobaciones cruzadas de bajo costo: campos obligatorios, longitudes, valores mínimos, formatos, fechas relativas entre sí y comparaciones con datos que ya están cargados en la aplicación. El objetivo es dar retroalimentación inmediata al usuario mientras llena el formulario.

El servidor valida las reglas de negocio y es su fuente de verdad. Cuando una regla no se cumple, responde con un código de error que el frontend traduce y muestra. Estas reglas no se reimplementan como validadores en el cliente, porque duplicar la lógica abre la puerta a que cliente y servidor discrepen. Salvo casos puntuales que se acuerden de forma explícita, las reglas de negocio se resuelven en el momento del envío, a través de la respuesta de error del comando.

## Los errores de validación usan el mismo contrato de códigos

Cada error de Signal Forms expone un identificador de tipo, su `kind`. Ese `kind` se usa como código de error, tomado del mismo catálogo que comparten el backend y el frontend, y se traduce en la plantilla mediante internacionalización, interpolando los parámetros cuando los haya. La consecuencia es que los errores de validación del cliente y los errores que devuelve el servidor se muestran con un único mecanismo: un código que la internacionalización convierte en texto legible. Esto aplica tanto a los validadores integrados como a los personalizados, y evita mantener dos sistemas de mensajes en paralelo. El contrato de códigos de error se describe en el documento [error-handling](./error-handling.md).

## Enlace con los componentes de PrimeNG

La directiva nativa `[formField]` de Signal Forms no compila sobre los componentes de PrimeNG 21, porque el chequeo de tipos de plantilla espera que el anfitrión implemente el contrato `FormUiControl`/`FormValueControl`, y esos componentes se construyen sobre `ControlValueAccessor` con inputs cuyos nombres y tipos colisionan con dicho contrato. Por eso, mientras PrimeNG no publique una versión alineada con Angular 22, los formularios se enlazan con el puente `@angular/forms/signals/compat`: cada campo se declara como un `SignalFormControl` con sus reglas de Signal Forms, se agrupa en un `FormGroup` y se enlaza en la plantilla con `formControlName`. Se conserva así la autoría de la validación en Signal Forms y se cumple la regla de no usar HTML crudo.

## Sin librerías de esquema

Signal Forms admite validación por esquema con librerías como Zod o Valibot. No se utilizan en el proyecto. Los validadores nativos y personalizados cubren las necesidades de validación sin añadir dependencias y mantienen las reglas junto a la definición del formulario.

## Pruebas

Los validadores personalizados son funciones y se prueban de forma aislada con Vitest. El comportamiento de los formularios, como la aparición de errores al tocar un campo o el bloqueo del envío cuando hay errores, se cubre con pruebas de componente, según la estrategia descrita en el documento [tdd](./tdd.md).
