# Manejo de Fechas y Horas

## Regla central

El backend opera y persiste exclusivamente en tiempo universal coordinado (UTC). Para almacenar y comparar fechas no necesita conocer la zona horaria del cliente, porque todo se guarda y se evalúa en UTC. El frontend, por su parte, trabaja con la zona horaria del cliente y la envía al backend en un header en cada petición. La conversión entre UTC y la hora local, necesaria únicamente para mostrar información al usuario, es responsabilidad exclusiva del frontend.

El flujo de una petición es el siguiente. El frontend incluye en cada solicitud un header que indica la zona horaria del cliente, por ejemplo `America/Bogota`. El backend realiza todos sus cálculos en UTC, tanto al obtener la hora actual como al evaluar las reglas de negocio sensibles a la hora. Finalmente, el frontend convierte las fechas recibidas en UTC a la hora local solo en el momento de mostrarlas.

## Motivación

Trabajar siempre en UTC en el backend evita las ambigüedades y los errores derivados de los cambios de horario, y permite que las reglas de negocio sensibles a la hora se evalúen de manera consistente. Entre esas reglas se encuentran la restricción de horario nocturno en fines de semana (RN03), la prohibición de reservar cuando el evento comienza en menos de una hora (RN04), el paso automático de un evento a estado completado cuando se supera su hora de fin (RN06) y la expiración de las reservas pendientes de pago.

## Implementación

En el backend se utiliza un servicio `IClock` que devuelve la hora actual en UTC. Al ser un servicio inyectable, permite controlar el tiempo en las pruebas y respeta la estrategia de desarrollo guiado por pruebas. Cuando una regla necesita la zona horaria del cliente, un middleware la obtiene a partir del header de la petición. En el frontend, un interceptor añade ese header de forma automática, y la capa de presentación se encarga de convertir las fechas para mostrarlas.

Como norma, en el backend nunca se utiliza la hora local del servidor. Toda obtención de la hora actual pasa por el servicio `IClock` y se expresa en UTC.
