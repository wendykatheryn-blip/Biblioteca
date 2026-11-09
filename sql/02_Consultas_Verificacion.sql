USE BibliotecaACA;
GO
SELECT * FROM dbo.Autores ORDER BY IdAutor;
SELECT * FROM dbo.Editoriales ORDER BY IdEditorial;
SELECT * FROM dbo.Usuarios ORDER BY IdUsuario;
SELECT l.IdLibro,l.Codigo,l.Titulo,CONCAT(a.Nombre,N' ',a.Apellido) AS Autor,e.Nombre AS Editorial
FROM dbo.Libros l JOIN dbo.Autores a ON a.IdAutor=l.IdAutor
JOIN dbo.Editoriales e ON e.IdEditorial=l.IdEditorial ORDER BY l.IdLibro;
SELECT p.IdPrestamo,u.Documento,CONCAT(u.Nombre,N' ',u.Apellido) AS Usuario,
    l.Codigo,l.Titulo,p.FechaPrestamo,p.FechaDevolucion,p.Estado
FROM dbo.Prestamos p JOIN dbo.Usuarios u ON u.IdUsuario=p.IdUsuario
JOIN dbo.Libros l ON l.IdLibro=p.IdLibro ORDER BY p.IdPrestamo;
/* Estas consultas de control deben devolver cero filas. */
SELECT IdLibro,COUNT(*) AS Activos FROM dbo.Prestamos WHERE Estado=N'Prestado'
GROUP BY IdLibro HAVING COUNT(*)>1;
SELECT * FROM dbo.Prestamos WHERE
    (Estado=N'Prestado' AND FechaDevolucion IS NOT NULL) OR
    (Estado=N'Devuelto' AND FechaDevolucion IS NULL) OR
    FechaDevolucion<FechaPrestamo;
GO
