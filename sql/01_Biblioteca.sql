/* Sistema de biblioteca - datos de prueba.
   Ejecutar completo en SSMS (F5). No elimina bases de datos ni registros.
   En DBeaver: ejecutar como script SQL, no como una sola sentencia.
*/
USE master;
GO
IF DB_ID(N'BibliotecaACA') IS NULL
    EXEC(N'CREATE DATABASE [BibliotecaACA]');
GO
USE BibliotecaACA;
GO
IF OBJECT_ID(N'dbo.Autores', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Autores (
        IdAutor INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Autores PRIMARY KEY,
        Nombre NVARCHAR(80) NOT NULL,
        Apellido NVARCHAR(80) NOT NULL,
        CONSTRAINT CK_Autores_Nombre CHECK (LEN(LTRIM(RTRIM(Nombre))) > 0),
        CONSTRAINT CK_Autores_Apellido CHECK (LEN(LTRIM(RTRIM(Apellido))) > 0)
    );
END;
GO
IF OBJECT_ID(N'dbo.Editoriales', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Editoriales (
        IdEditorial INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Editoriales PRIMARY KEY,
        Nombre NVARCHAR(120) NOT NULL CONSTRAINT UQ_Editoriales_Nombre UNIQUE,
        CONSTRAINT CK_Editoriales_Nombre CHECK (LEN(LTRIM(RTRIM(Nombre))) > 0)
    );
END;
GO
IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios (
        IdUsuario INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Usuarios PRIMARY KEY,
        Documento NVARCHAR(30) NOT NULL CONSTRAINT UQ_Usuarios_Documento UNIQUE,
        Nombre NVARCHAR(80) NOT NULL,
        Apellido NVARCHAR(80) NOT NULL,
        Telefono NVARCHAR(30) NULL,
        Correo NVARCHAR(150) NULL,
        CONSTRAINT CK_Usuarios_Documento CHECK (LEN(LTRIM(RTRIM(Documento))) > 0),
        CONSTRAINT CK_Usuarios_Nombre CHECK (LEN(LTRIM(RTRIM(Nombre))) > 0),
        CONSTRAINT CK_Usuarios_Apellido CHECK (LEN(LTRIM(RTRIM(Apellido))) > 0)
    );
END;
GO
IF OBJECT_ID(N'dbo.Libros', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Libros (
        IdLibro INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Libros PRIMARY KEY,
        Codigo NVARCHAR(30) NOT NULL CONSTRAINT UQ_Libros_Codigo UNIQUE,
        Titulo NVARCHAR(180) NOT NULL,
        IdAutor INT NOT NULL,
        IdEditorial INT NOT NULL,
        CONSTRAINT FK_Libros_Autores FOREIGN KEY (IdAutor) REFERENCES dbo.Autores(IdAutor),
        CONSTRAINT FK_Libros_Editoriales FOREIGN KEY (IdEditorial) REFERENCES dbo.Editoriales(IdEditorial),
        CONSTRAINT CK_Libros_Codigo CHECK (LEN(LTRIM(RTRIM(Codigo))) > 0),
        CONSTRAINT CK_Libros_Titulo CHECK (LEN(LTRIM(RTRIM(Titulo))) > 0)
    );
END;
GO
IF OBJECT_ID(N'dbo.Prestamos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Prestamos (
        IdPrestamo INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Prestamos PRIMARY KEY,
        IdUsuario INT NOT NULL,
        IdLibro INT NOT NULL,
        FechaPrestamo DATE NOT NULL,
        FechaDevolucion DATE NULL,
        Estado NVARCHAR(10) NOT NULL CONSTRAINT DF_Prestamos_Estado DEFAULT N'Prestado',
        CONSTRAINT FK_Prestamos_Usuarios FOREIGN KEY (IdUsuario) REFERENCES dbo.Usuarios(IdUsuario),
        CONSTRAINT FK_Prestamos_Libros FOREIGN KEY (IdLibro) REFERENCES dbo.Libros(IdLibro),
        CONSTRAINT CK_Prestamos_Estado CHECK (Estado IN (N'Prestado', N'Devuelto')),
        CONSTRAINT CK_Prestamos_Fechas CHECK (FechaDevolucion IS NULL OR FechaDevolucion >= FechaPrestamo),
        CONSTRAINT CK_Prestamos_Coherencia CHECK (
            (Estado = N'Prestado' AND FechaDevolucion IS NULL) OR
            (Estado = N'Devuelto' AND FechaDevolucion IS NOT NULL))
    );
END;
GO
/* Cada fila de Libros representa un ejemplar: un solo préstamo activo por ejemplar. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Prestamos_LibroActivo'
               AND object_id = OBJECT_ID(N'dbo.Prestamos'))
    CREATE UNIQUE INDEX UX_Prestamos_LibroActivo ON dbo.Prestamos(IdLibro)
    WHERE Estado = N'Prestado';
GO
/* Datos de prueba; los INSERT no duplican estos registros al repetir el script. */
IF NOT EXISTS (SELECT 1 FROM dbo.Autores WHERE Nombre=N'Lucía' AND Apellido=N'Pérez')
    INSERT dbo.Autores(Nombre,Apellido) VALUES(N'Lucía',N'Pérez');
IF NOT EXISTS (SELECT 1 FROM dbo.Autores WHERE Nombre=N'Carlos' AND Apellido=N'Ramírez')
    INSERT dbo.Autores(Nombre,Apellido) VALUES(N'Carlos',N'Ramírez');
IF NOT EXISTS (SELECT 1 FROM dbo.Editoriales WHERE Nombre=N'Editorial Aurora')
    INSERT dbo.Editoriales(Nombre) VALUES(N'Editorial Aurora');
IF NOT EXISTS (SELECT 1 FROM dbo.Editoriales WHERE Nombre=N'Editorial Horizonte')
    INSERT dbo.Editoriales(Nombre) VALUES(N'Editorial Horizonte');
IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Documento=N'PRUEBA-001')
    INSERT dbo.Usuarios(Documento,Nombre,Apellido,Telefono,Correo)
    VALUES(N'PRUEBA-001',N'Laura',N'Gómez',N'3000000001',N'laura@example.com');
IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Documento=N'PRUEBA-002')
    INSERT dbo.Usuarios(Documento,Nombre,Apellido,Telefono,Correo)
    VALUES(N'PRUEBA-002',N'Daniel',N'Ruiz',N'3000000002',N'daniel@example.com');
DECLARE @Autor1 INT=(SELECT MIN(IdAutor) FROM dbo.Autores WHERE Nombre=N'Lucía' AND Apellido=N'Pérez');
DECLARE @Autor2 INT=(SELECT MIN(IdAutor) FROM dbo.Autores WHERE Nombre=N'Carlos' AND Apellido=N'Ramírez');
DECLARE @Editorial1 INT=(SELECT IdEditorial FROM dbo.Editoriales WHERE Nombre=N'Editorial Aurora');
DECLARE @Editorial2 INT=(SELECT IdEditorial FROM dbo.Editoriales WHERE Nombre=N'Editorial Horizonte');
IF NOT EXISTS (SELECT 1 FROM dbo.Libros WHERE Codigo=N'BIB-001')
    INSERT dbo.Libros(Codigo,Titulo,IdAutor,IdEditorial) VALUES(N'BIB-001',N'Introducción a la programación',@Autor1,@Editorial1);
IF NOT EXISTS (SELECT 1 FROM dbo.Libros WHERE Codigo=N'BIB-002')
    INSERT dbo.Libros(Codigo,Titulo,IdAutor,IdEditorial) VALUES(N'BIB-002',N'Fundamentos de bases de datos',@Autor2,@Editorial2);
IF NOT EXISTS (SELECT 1 FROM dbo.Libros WHERE Codigo=N'BIB-003')
    INSERT dbo.Libros(Codigo,Titulo,IdAutor,IdEditorial) VALUES(N'BIB-003',N'Lecturas para aprender',@Autor1,@Editorial2);
DECLARE @Libro1 INT=(SELECT IdLibro FROM dbo.Libros WHERE Codigo=N'BIB-001');
DECLARE @Libro2 INT=(SELECT IdLibro FROM dbo.Libros WHERE Codigo=N'BIB-002');
DECLARE @Usuario1 INT=(SELECT IdUsuario FROM dbo.Usuarios WHERE Documento=N'PRUEBA-001');
DECLARE @Usuario2 INT=(SELECT IdUsuario FROM dbo.Usuarios WHERE Documento=N'PRUEBA-002');
DECLARE @Hoy DATE=CONVERT(DATE,GETDATE());
IF NOT EXISTS (SELECT 1 FROM dbo.Prestamos WHERE IdLibro=@Libro1)
    INSERT dbo.Prestamos(IdUsuario,IdLibro,FechaPrestamo,FechaDevolucion,Estado)
    VALUES(@Usuario1,@Libro1,@Hoy,NULL,N'Prestado');
IF NOT EXISTS (SELECT 1 FROM dbo.Prestamos WHERE IdLibro=@Libro2)
    INSERT dbo.Prestamos(IdUsuario,IdLibro,FechaPrestamo,FechaDevolucion,Estado)
    VALUES(@Usuario2,@Libro2,DATEADD(DAY,-2,@Hoy),@Hoy,N'Devuelto');
GO
SELECT N'Autores' AS Tabla,COUNT(*) AS Registros FROM dbo.Autores
UNION ALL SELECT N'Editoriales',COUNT(*) FROM dbo.Editoriales
UNION ALL SELECT N'Usuarios',COUNT(*) FROM dbo.Usuarios
UNION ALL SELECT N'Libros',COUNT(*) FROM dbo.Libros
UNION ALL SELECT N'Prestamos',COUNT(*) FROM dbo.Prestamos;
GO
