# Sistema de Gestión de Biblioteca

## Contexto del sistema

La biblioteca municipal necesita modernizar su sistema de gestión de préstamos de libros. Los socios pueden retirar libros por un período determinado según su tipo de membresía. Cuando un libro no está disponible, los socios pueden reservarlo para cuando sea devuelto.

El sistema debe registrar todos los préstamos, devoluciones y reservas, controlando las multas por demora y los límites de cada tipo de socio.

---

## Modelo de datos y base de datos

Diseñar e implementar el modelo de datos del sistema. Las entidades principales son:

### Libro
- `ISBN` — identificador único del libro (texto, PK).
- `Título` — nombre del libro.
- `Autor` — nombre del autor.
- `Género` — categoría del libro (ej: "Ficción", "Ciencia", "Historia").
- `CantidadCopias` — cantidad total de ejemplares que posee la biblioteca.

### Socio
- `NroSocio` — número entero único que identifica al socio (PK).
- `Nombre` y `Apellido`. 
- `Email`.
- `TipoSocio` — tabla separada que contiene los tipos posibles.
- `Activo` — indica si el socio está habilitado para realizar préstamos.

### Prestamo
- `Socio` — el socio que retiró el libro.
- `Libro` — el libro prestado.
- `FechaPrestamo` — fecha en que se retiró el libro.
- `FechaVencimiento` — fecha máxima de devolución.
- `FechaDevolucion` — fecha real de devolución (nula si aún no fue devuelto).
- `Estado` — tabla separada: `Activo`, `Devuelto`, `Vencido`.

### Reserva
- `Socio` — el socio que realizó la reserva.
- `Libro` — el libro reservado.
- `FechaReserva` — fecha en que se realizó la reserva.
- `Estado` — tabla separada: `Pendiente`, `Cumplida`, `Cancelada`.

### Requerimientos

- Crear el **script SQL** (`biblioteca.sql`) con la creación de todas las tablas e inserción de datos iniciales:
  - Mínimo 5 libros con varias copias.
  - Mínimo 5 socios de distintos tipos.
  - Algunos préstamos activos y vencidos.
- Implementar las clases del modelo en C# y el `DbContext` con Entity Framework Core correctamente configurado.
- Al iniciar, la aplicación debe mostrar en consola la lista de todos los libros disponibles.

---

## Parte 2 — Tipos de socio y reglas de préstamo

Existen tres tipos de socio, cada uno con distintos límites:

| Tipo de socio | Máx. libros simultáneos | Días de préstamo | Multa por día de demora |
|---|---|---|---|
| Común | 3 libros | 7 días | $150 |
| Estudiante | 5 libros | 14 días | $75 |
| Docente | 8 libros | 30 días | $50 |

### Reglas de negocio

| Regla | Descripción |
|---|---|
| RN-01 | Un socio inactivo no puede realizar préstamos ni reservas. |
| RN-02 | Un socio con multas pendientes no puede retirar nuevos libros hasta abonarlas. |
| RN-03 | No se puede prestar un libro si no hay copias disponibles. En ese caso se ofrece la opción de reservar. |
| RN-04 | Un socio no puede superar su límite de libros simultáneos en préstamo. |
| RN-05 | La fecha de vencimiento se calcula automáticamente según el tipo de socio al momento del préstamo. |
| RN-06 | Al registrar una devolución con demora, se calcula y registra la multa automáticamente. |
| RN-07 | Cuando se devuelve un libro reservado, la reserva más antigua pendiente pasa a Cumplida y se notifica por consola. |
| RN-08 | Un socio no puede tener más de una reserva activa para el mismo libro. |

### Requerimientos

- Implementar el flujo completo de **préstamo** desde consola: buscar libro por título o autor, verificar disponibilidad, registrar el préstamo.
- Implementar el flujo de **devolución**: buscar el préstamo activo del socio, registrar la fecha de devolución y calcular la multa si corresponde.
- Implementar el flujo de **reserva**: cuando no hay copias disponibles, ofrecer reservar el libro.
- Mostrar en consola el **detalle del socio**: préstamos activos, historial de devoluciones y multas pendientes.

---

## Parte 3 — Consultas y reportes

Implementar las siguientes consultas accesibles desde el menú de consola:

- **Libros más prestados** — listar los 5 libros con mayor cantidad de préstamos históricos.
- **Socios con multas pendientes** — listar todos los socios que tienen multa sin abonar, mostrando el monto total.
- **Préstamos vencidos** — listar todos los préstamos cuya fecha de vencimiento ya pasó y aún no fueron devueltos.
- **Disponibilidad de un libro** — dado un ISBN o título, mostrar cuántas copias están disponibles y si hay reservas pendientes.
- **Historial de un socio** — dado un número de socio, mostrar todos sus préstamos y reservas con sus estados.

> **Importante:** todas las consultas deben ejecutarse contra la base de datos con Entity Framework.

---

## Consigna adicional (Solo para el grupo de 3)

> Esta sección es **obligatoria únicamente** para el grupo de tres integrantes. Para el resto de equipos es completamente opcional.

### Sistema de renovación de préstamos

Implementar la funcionalidad de renovación de préstamos con las siguientes reglas:

- Un préstamo puede renovarse **una única vez**, extendiendo la fecha de vencimiento por la misma cantidad de días del préstamo original.
- No se puede renovar un préstamo que **ya fue renovado** anteriormente.
- No se puede renovar un préstamo **vencido**.
- No se puede renovar un préstamo si el libro tiene **reservas pendientes** de otros socios.
- La renovación queda registrada en la base de datos con una columna adicional en la tabla de préstamos: `Renovado` (booleano).

### Ranking de socios

Implementar un reporte accesible desde el menú que muestre el **ranking de los 10 socios más activos**, ordenados por cantidad total de préstamos históricos (incluyendo los devueltos). Para cada socio mostrar: nombre, tipo, cantidad de préstamos y si tiene multas pendientes.

---

## Importante
- Cada clase debe estar en un archivo separado que se llame como la clase.
- Se evaluara el uso de git, es decir, que tengan mas de un commit, mensajes de los mismos, ramas y la contribución de cada alumno.
- **Aclaracion**: los repositorios son publicos y de libre acceso, dicho esto, la entrega de dos (o mas) trabajos *iguales* sera motivo de aplazo sin excepción.

## Para la entrega

- Completar este [form](https://forms.gle/5xySkjoNBW1AsPBf7) con el link del repositorio y nombre de los integrantes.
- En el readme de su repositorio escribir los nombre y apellido de los integrantes y nombrar el proyecto de .Net de esta manera: Biblioteca_Apellido1_Apellido2.
- Fecha **tentativa** para la entrega Viernes 26/6(Inclusive).
 