-- ============================================================
-- Sistema de Gestión de Biblioteca
-- Script SQL para SQLite
-- ============================================================

PRAGMA foreign_keys = ON;

-- ============================================================
-- TABLAS DE CATÁLOGO / LOOKUP
-- ============================================================

CREATE TABLE IF NOT EXISTS TipoSocio (
    Id           INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre       TEXT    NOT NULL UNIQUE,
    MaxLibros    INTEGER NOT NULL,
    DiasPrestamo INTEGER NOT NULL,
    MultaPorDia  REAL    NOT NULL
);

CREATE TABLE IF NOT EXISTS EstadoPrestamo (
    Id     INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre TEXT    NOT NULL UNIQUE   -- 'Activo', 'Devuelto', 'Vencido'
);

CREATE TABLE IF NOT EXISTS EstadoReserva (
    Id     INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre TEXT    NOT NULL UNIQUE   -- 'Pendiente', 'Cumplida', 'Cancelada'
);

-- ============================================================
-- ENTIDADES PRINCIPALES
-- ============================================================

CREATE TABLE IF NOT EXISTS Libro (
    ISBN           TEXT    PRIMARY KEY,
    Titulo         TEXT    NOT NULL,
    Autor          TEXT    NOT NULL,
    Genero         TEXT    NOT NULL,
    CantidadCopias INTEGER NOT NULL CHECK (CantidadCopias >= 0)
);

CREATE TABLE IF NOT EXISTS Socio (
    NroSocio   INTEGER PRIMARY KEY,
    Nombre     TEXT    NOT NULL,
    Apellido   TEXT    NOT NULL,
    Email      TEXT    NOT NULL,
    TipoSocioId INTEGER NOT NULL,
    Activo     INTEGER NOT NULL DEFAULT 1,   -- 1 = true, 0 = false
    FOREIGN KEY (TipoSocioId) REFERENCES TipoSocio(Id)
);

CREATE TABLE IF NOT EXISTS Prestamo (
    Id               INTEGER PRIMARY KEY AUTOINCREMENT,
    SocioId          INTEGER NOT NULL,
    LibroISBN        TEXT    NOT NULL,
    FechaPrestamo    TEXT    NOT NULL,   -- ISO 8601: 'YYYY-MM-DD'
    FechaVencimiento TEXT    NOT NULL,
    FechaDevolucion  TEXT,               -- NULL si no fue devuelto aún
    EstadoPrestamoId INTEGER NOT NULL,
    Multa            REAL,               -- NULL si no hay multa
    FOREIGN KEY (SocioId)          REFERENCES Socio(NroSocio),
    FOREIGN KEY (LibroISBN)        REFERENCES Libro(ISBN),
    FOREIGN KEY (EstadoPrestamoId) REFERENCES EstadoPrestamo(Id)
);

CREATE TABLE IF NOT EXISTS Reserva (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    SocioId         INTEGER NOT NULL,
    LibroISBN       TEXT    NOT NULL,
    FechaReserva    TEXT    NOT NULL,   -- ISO 8601: 'YYYY-MM-DD'
    EstadoReservaId INTEGER NOT NULL,
    FOREIGN KEY (SocioId)         REFERENCES Socio(NroSocio),
    FOREIGN KEY (LibroISBN)       REFERENCES Libro(ISBN),
    FOREIGN KEY (EstadoReservaId) REFERENCES EstadoReserva(Id)
);

-- ============================================================
-- DATOS INICIALES — CATÁLOGOS
-- ============================================================

INSERT INTO TipoSocio (Nombre, MaxLibros, DiasPrestamo, MultaPorDia) VALUES
    ('Común',      3, 7,  150.0),
    ('Estudiante', 5, 14,  75.0),
    ('Docente',    8, 30,  50.0);

INSERT INTO EstadoPrestamo (Nombre) VALUES
    ('Activo'),
    ('Devuelto'),
    ('Vencido');

INSERT INTO EstadoReserva (Nombre) VALUES
    ('Pendiente'),
    ('Cumplida'),
    ('Cancelada');

-- ============================================================
-- DATOS INICIALES — LIBROS (mínimo 5)
-- ============================================================

INSERT INTO Libro (ISBN, Titulo, Autor, Genero, CantidadCopias) VALUES
    ('978-0-061-96436-9', 'Cien Años de Soledad',         'Gabriel García Márquez', 'Ficción',   4),
    ('978-0-452-28423-4', '1984',                          'George Orwell',           'Ficción',   3),
    ('978-0-743-27356-5', 'El Gran Gatsby',                'F. Scott Fitzgerald',     'Ficción',   2),
    ('978-0-307-47347-5', 'El Código Da Vinci',            'Dan Brown',               'Thriller',  5),
    ('978-0-385-33348-1', 'Cosmos',                        'Carl Sagan',              'Ciencia',   3),
    ('978-0-142-41104-1', 'Crimen y Castigo',              'Fiódor Dostoyevski',      'Ficción',   2),
    ('978-0-141-18776-1', 'Sapiens',                       'Yuval Noah Harari',       'Historia',  4),
    ('978-0-525-55360-5', 'El Alquimista',                 'Paulo Coelho',            'Ficción',   3);

-- ============================================================
-- DATOS INICIALES — SOCIOS (mínimo 5, de distintos tipos)
-- ============================================================
-- TipoSocio: 1=Común, 2=Estudiante, 3=Docente

INSERT INTO Socio (NroSocio, Nombre, Apellido, Email, TipoSocioId, Activo) VALUES
    (1001, 'Ana',      'López',     'ana.lopez@mail.com',     1, 1),  -- Común, activo
    (1002, 'Bruno',    'Martínez',  'bruno.m@mail.com',       2, 1),  -- Estudiante, activo
    (1003, 'Carla',    'Rodríguez', 'carla.r@mail.com',       3, 1),  -- Docente, activo
    (1004, 'Diego',    'Fernández', 'diego.f@mail.com',       1, 1),  -- Común, activo
    (1005, 'Elena',    'Gómez',     'elena.g@mail.com',       2, 1),  -- Estudiante, activo
    (1006, 'Federico', 'Torres',    'fede.t@mail.com',        3, 1),  -- Docente, activo
    (1007, 'Gabriela', 'Díaz',      'gabi.d@mail.com',        1, 0);  -- Común, INACTIVO

-- ============================================================
-- DATOS INICIALES — PRÉSTAMOS
-- EstadoPrestamo: 1=Activo, 2=Devuelto, 3=Vencido
-- ============================================================

INSERT INTO Prestamo (SocioId, LibroISBN, FechaPrestamo, FechaVencimiento, FechaDevolucion, EstadoPrestamoId, Multa) VALUES
    -- Préstamos ACTIVOS (dentro del plazo)
    (1001, '978-0-061-96436-9', '2026-06-20', '2026-06-27', NULL, 1, NULL),
    (1002, '978-0-452-28423-4', '2026-06-15', '2026-06-29', NULL, 1, NULL),
    (1003, '978-0-307-47347-5', '2026-06-01', '2026-07-01', NULL, 1, NULL),
    (1005, '978-0-385-33348-1', '2026-06-18', '2026-07-02', NULL, 1, NULL),

    -- Préstamos VENCIDOS (fecha de vencimiento ya pasó, sin devolver)
    (1004, '978-0-743-27356-5', '2026-06-01', '2026-06-08', NULL, 3, NULL),
    (1001, '978-0-142-41104-1', '2026-05-25', '2026-06-01', NULL, 3, NULL),
    (1002, '978-0-525-55360-5', '2026-06-01', '2026-06-15', NULL, 3, NULL),

    -- Préstamos DEVUELTOS (historial)
    (1003, '978-0-141-18776-1', '2026-05-01', '2026-05-31', '2026-05-28', 2, NULL),
    (1004, '978-0-061-96436-9', '2026-05-10', '2026-05-17', '2026-05-20', 2, 450.0),  -- devuelto con multa (3 días × $150)
    (1006, '978-0-385-33348-1', '2026-04-01', '2026-05-01', '2026-04-29', 2, NULL);

-- ============================================================
-- DATOS INICIALES — RESERVAS
-- EstadoReserva: 1=Pendiente, 2=Cumplida, 3=Cancelada
-- ============================================================

INSERT INTO Reserva (SocioId, LibroISBN, FechaReserva, EstadoReservaId) VALUES
    -- Reservas PENDIENTES (libros sin copias disponibles)
    (1005, '978-0-743-27356-5', '2026-06-10', 1),
    (1006, '978-0-142-41104-1', '2026-06-12', 1),
    -- Reserva CUMPLIDA (historial)
    (1001, '978-0-141-18776-1', '2026-04-28', 2),
    -- Reserva CANCELADA (historial)
    (1007, '978-0-307-47347-5', '2026-06-05', 3);
