# Modelado de Base de Datos

## Configuración con Fluent API, no Data Annotations

Las tablas no se configuran con Data Annotations, es decir, no se anotan las entidades con atributos de persistencia. Toda la configuración se expresa con la Fluent API de Entity Framework Core, mediante una clase de configuración por entidad que implementa `IEntityTypeConfiguration<T>`.

Esta decisión tiene varias razones. Mantiene las entidades del dominio limpias, como objetos sin dependencias de infraestructura, lo que respeta la regla de dependencia de la arquitectura, según la cual el dominio no debe conocer a Entity Framework Core. Además, centraliza la configuración de cada entidad en un único lugar, permite expresar con naturalidad mapeos que las anotaciones manejan mal o no cubren, como las claves compuestas, los índices, las restricciones, las conversiones de valores y el token de concurrencia basado en `xmin`, y mantiene el contexto de datos ordenado.

## Índices con criterio

Se definen los índices que aporten un rendimiento real, de acuerdo con los patrones de acceso de la aplicación. Esto incluye las columnas por las que se filtra y se busca en el listado de eventos (RF-02), las claves foráneas y las columnas por las que se ordena o se agrupa.

Al mismo tiempo, no se satura la tabla de índices. Cada índice tiene un costo en las operaciones de escritura y en el espacio de almacenamiento, de modo que solo se crean aquellos que se justifican por consultas concretas, y no de forma preventiva o indiscriminada.

## Sin columnas repetidas entre los índices de una tabla

Si una misma columna aparece repetida en varios índices de la misma tabla, es señal de un diseño incorrecto. La situación más común es un índice cuyas columnas ya están cubiertas por el prefijo de otro índice: en ese caso el índice no aporta nada y solo penaliza la escritura.

Por eso se prefiere diseñar índices compuestos bien pensados, con las columnas en el orden que mejor sirva a las consultas, en lugar de crear varios índices que repiten las mismas columnas. Antes de agregar un índice se revisa que ninguna de sus columnas quede duplicada de forma redundante respecto de los índices ya existentes en esa tabla.
