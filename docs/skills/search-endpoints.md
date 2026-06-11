# Endpoints de Búsqueda para Selección

Cuando una vista necesita poblar un selector de entidades relacionadas —por ejemplo, elegir el lugar al crear un evento—, el backend expone esa búsqueda en un endpoint cuya ruta termina en `/search`. Para los lugares, ese endpoint es `GET /api/v1/venues/search`.

Estos endpoints devuelven únicamente los datos necesarios para la selección, es decir, el identificador y los campos que se muestran. Admiten un término de búsqueda opcional y devuelven un conjunto **acotado** de coincidencias, resuelto en la base de datos según el skill [data-access](./data-access.md).

A diferencia de un listado paginado, un endpoint de selección **no calcula el total de resultados**: un selector no muestra controles de paginación, así que basta con limitar las coincidencias (por ejemplo, las primeras veinte). Calcular un total que la interfaz no usa sería trabajo innecesario.

La terminación `/search` distingue estos endpoints de los listados principales de cada recurso y establece una convención uniforme para todas las selecciones de campos relacionados en el sistema.
