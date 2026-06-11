# Acceso a Datos

## La base de datos resuelve, no la memoria

El principio es que sea la base de datos quien resuelva el trabajo sobre los datos siempre que sea posible. Esto no se limita a la obtención de información: abarca también los cálculos, los procesos y las búsquedas. Los filtros, las búsquedas parciales, los ordenamientos, la paginación, las sumas, los conteos, los promedios y cualquier otra operación sobre conjuntos de datos se expresan como parte de la consulta, de modo que PostgreSQL los ejecute.

Traer colecciones completas a memoria para filtrarlas, ordenarlas, recorrerlas o calcular sobre ellas en código desperdicia recursos, no escala a medida que crecen los datos y, en el peor de los casos, provoca evaluación del lado del cliente. La base de datos está diseñada y optimizada precisamente para resolver ese tipo de trabajo, y por eso es donde debe ocurrir.

Esta preferencia es especialmente relevante en dos requerimientos del enunciado. El listado de eventos con filtros opcionales (RF-02) aplica cada filtro y cada búsqueda directamente en la consulta. El reporte de ocupación (RF-06) calcula sus sumas, conteos y porcentajes mediante agregaciones en la base de datos, sin traer las reservas a memoria para sumarlas en código.

Como regla general, todo listado se pagina en el servidor, no solo el de eventos. La base de datos resuelve la paginación, y la consulta devuelve una sola página junto con los metadatos necesarios, como el total de elementos. Nunca se traen todos los registros a memoria para paginarlos en código.

## Sin SQL puro

No se utiliza SQL puro, es decir, no se escriben cadenas de SQL a mano ni se recurre a mecanismos como `FromSqlRaw`. Todo el acceso a datos se expresa con LINQ sobre Entity Framework Core.

Ambas formas de escribir LINQ son válidas y se elige en cada caso la que resulte más legible: la sintaxis de método (method syntax), encadenando operadores como `Where`, `OrderBy` o `Select`, y la sintaxis de consulta (query syntax), con la forma `from ... where ... select`. En los dos casos, Entity Framework Core traduce la expresión a SQL parametrizado, lo que preserva la seguridad frente a inyección de SQL y mantiene el código independiente del motor.

## Ante la duda, aclarar

Si en algún momento no resulta evidente cómo expresar un cálculo, un proceso o una búsqueda con LINQ sin caer en SQL puro o en procesamiento en memoria, no se improvisa una solución. Se detiene el trabajo y se aclara la duda para llegar a una conclusión acordada, en lugar de avanzar con una alternativa que contradiga estos principios.
