using System.Data;
using Microsoft.Data.SqlClient;
using Biblioteca.Entidades;

namespace Biblioteca.Datos;

public sealed class BibliotecaRepositorio
{
    private readonly string cadenaConexion;
    public BibliotecaRepositorio(string cadenaConexion) { this.cadenaConexion = cadenaConexion; }

    private SqlConnection Abrir()
    {
        var conexion = new SqlConnection(cadenaConexion);
        try { conexion.Open(); return conexion; }
        catch { conexion.Dispose(); throw; }
    }

    private static SqlParameter P(string nombre, SqlDbType tipo, object? valor, int longitud = 0)
    {
        var parametro = new SqlParameter(nombre, tipo) { Value = valor ?? DBNull.Value };
        if (longitud > 0) parametro.Size = longitud;
        return parametro;
    }

    private DataTable Consultar(string sql, params SqlParameter[] parametros)
    {
        using var conexion = Abrir();
        using var comando = new SqlCommand(sql, conexion);
        comando.Parameters.AddRange(parametros);
        using var adaptador = new SqlDataAdapter(comando);
        var resultado = new DataTable();
        adaptador.Fill(resultado);
        return resultado;
    }

    public DataTable Listar(TipoCatalogo tipo, string buscar = "")
    {
        // Los identificadores provienen de este switch, nunca de entradas del usuario.
        string sql = tipo switch
        {
            TipoCatalogo.Autores => "SELECT IdAutor AS Id,Nombre,Apellido FROM dbo.Autores WHERE CONCAT(Nombre,N' ',Apellido) LIKE @buscar ORDER BY Nombre,Apellido",
            TipoCatalogo.Editoriales => "SELECT IdEditorial AS Id,Nombre FROM dbo.Editoriales WHERE Nombre LIKE @buscar ORDER BY Nombre",
            TipoCatalogo.Usuarios => "SELECT IdUsuario AS Id,Documento,Nombre,Apellido,Telefono,Correo FROM dbo.Usuarios WHERE CONCAT(Documento,N' ',Nombre,N' ',Apellido) LIKE @buscar ORDER BY Nombre,Apellido",
            TipoCatalogo.Libros => @"SELECT l.IdLibro AS Id,l.Codigo,l.Titulo,l.IdAutor,l.IdEditorial,
                CONCAT(a.Nombre,N' ',a.Apellido) AS Autor,e.Nombre AS Editorial,
                CASE WHEN EXISTS(SELECT 1 FROM dbo.Prestamos p WHERE p.IdLibro=l.IdLibro AND p.Estado=N'Prestado')
                THEN N'Prestado' ELSE N'Disponible' END AS Disponibilidad
                FROM dbo.Libros l JOIN dbo.Autores a ON a.IdAutor=l.IdAutor
                JOIN dbo.Editoriales e ON e.IdEditorial=l.IdEditorial
                WHERE CONCAT(l.Codigo,N' ',l.Titulo,N' ',a.Nombre,N' ',a.Apellido) LIKE @buscar ORDER BY l.Titulo",
            _ => throw new ArgumentOutOfRangeException(nameof(tipo))
        };
        return Consultar(sql, P("@buscar", SqlDbType.NVarChar, "%" + buscar + "%", 220));
    }

    public DataTable Opciones(TipoCatalogo tipo)
    {
        string sql = tipo switch
        {
            TipoCatalogo.Autores => "SELECT IdAutor AS Id,CONCAT(Nombre,N' ',Apellido) AS Descripcion FROM dbo.Autores ORDER BY Nombre,Apellido",
            TipoCatalogo.Editoriales => "SELECT IdEditorial AS Id,Nombre AS Descripcion FROM dbo.Editoriales ORDER BY Nombre",
            TipoCatalogo.Usuarios => "SELECT IdUsuario AS Id,CONCAT(Documento,N' - ',Nombre,N' ',Apellido) AS Descripcion FROM dbo.Usuarios ORDER BY Nombre,Apellido",
            TipoCatalogo.Libros => "SELECT IdLibro AS Id,CONCAT(Codigo,N' - ',Titulo) AS Descripcion FROM dbo.Libros ORDER BY Titulo",
            _ => throw new ArgumentOutOfRangeException(nameof(tipo))
        };
        return Consultar(sql);
    }

    public bool Existe(TipoCatalogo tipo, int id)
    {
        using var conexion = Abrir();
        using var comando = new SqlCommand($"SELECT COUNT(*) FROM dbo.{Catalogos.Tabla(tipo)} WHERE {Catalogos.Clave(tipo)}=@id", conexion);
        comando.Parameters.Add(P("@id", SqlDbType.Int, id));
        return Convert.ToInt32(comando.ExecuteScalar()) == 1;
    }

    public void Guardar(Entidad entidad)
    {
        TipoCatalogo tipo;
        var valores = new List<SqlParameter>();
        switch (entidad)
        {
            case Autor a:
                tipo = TipoCatalogo.Autores;
                valores.Add(P("@Nombre", SqlDbType.NVarChar, a.Nombre, 80));
                valores.Add(P("@Apellido", SqlDbType.NVarChar, a.Apellido, 80)); break;
            case Editorial e:
                tipo = TipoCatalogo.Editoriales;
                valores.Add(P("@Nombre", SqlDbType.NVarChar, e.Nombre, 120)); break;
            case Usuario u:
                tipo = TipoCatalogo.Usuarios;
                valores.Add(P("@Documento", SqlDbType.NVarChar, u.Documento, 30));
                valores.Add(P("@Nombre", SqlDbType.NVarChar, u.Nombre, 80));
                valores.Add(P("@Apellido", SqlDbType.NVarChar, u.Apellido, 80));
                valores.Add(P("@Telefono", SqlDbType.NVarChar, u.Telefono, 30));
                valores.Add(P("@Correo", SqlDbType.NVarChar, u.Correo, 150)); break;
            case Libro l:
                tipo = TipoCatalogo.Libros;
                valores.Add(P("@Codigo", SqlDbType.NVarChar, l.Codigo, 30));
                valores.Add(P("@Titulo", SqlDbType.NVarChar, l.Titulo, 180));
                valores.Add(P("@IdAutor", SqlDbType.Int, l.IdAutor));
                valores.Add(P("@IdEditorial", SqlDbType.Int, l.IdEditorial)); break;
            default: throw new ArgumentException("Tipo de registro no admitido.");
        }
        var nombres = valores.Select(p => p.ParameterName[1..]).ToArray();
        string sql = entidad.Id == 0
            ? $"INSERT dbo.{Catalogos.Tabla(tipo)} ({string.Join(",", nombres)}) VALUES ({string.Join(",", valores.Select(p => p.ParameterName))})"
            : $"UPDATE dbo.{Catalogos.Tabla(tipo)} SET {string.Join(",", nombres.Select(n => n + "=@" + n))} WHERE {Catalogos.Clave(tipo)}=@Id";
        if (entidad.Id != 0) valores.Add(P("@Id", SqlDbType.Int, entidad.Id));
        using var conexion = Abrir();
        using var comando = new SqlCommand(sql, conexion);
        comando.Parameters.AddRange(valores.ToArray());
        if (comando.ExecuteNonQuery() != 1) throw new InvalidOperationException("El registro ya no existe. Actualiza la lista.");
    }

    public void Eliminar(TipoCatalogo tipo, int id)
    {
        using var conexion = Abrir();
        using var comando = new SqlCommand($"DELETE FROM dbo.{Catalogos.Tabla(tipo)} WHERE {Catalogos.Clave(tipo)}=@id", conexion);
        comando.Parameters.Add(P("@id", SqlDbType.Int, id));
        if (comando.ExecuteNonQuery() != 1) throw new InvalidOperationException("El registro ya no existe.");
    }

    public DataTable ListarPrestamos(string buscar = "", string? estado = null) => Consultar(@"
        SELECT p.IdPrestamo AS Id,p.IdUsuario,p.IdLibro,u.Documento,
        CONCAT(u.Nombre,N' ',u.Apellido) AS Usuario,l.Codigo,l.Titulo AS Libro,
        p.FechaPrestamo,p.FechaDevolucion,p.Estado
        FROM dbo.Prestamos p JOIN dbo.Usuarios u ON u.IdUsuario=p.IdUsuario
        JOIN dbo.Libros l ON l.IdLibro=p.IdLibro
        WHERE CONCAT(u.Documento,N' ',u.Nombre,N' ',u.Apellido,N' ',l.Codigo,N' ',l.Titulo,N' ',p.Estado) LIKE @buscar
        AND (@estado IS NULL OR p.Estado=@estado) ORDER BY p.IdPrestamo DESC",
        P("@buscar", SqlDbType.NVarChar, "%" + buscar + "%", 220), P("@estado", SqlDbType.NVarChar, estado, 10));

    public void GuardarPrestamo(Prestamo p)
    {
        using var conexion = Abrir();
        using var transaccion = conexion.BeginTransaction(IsolationLevel.Serializable);
        // Serializa préstamos del mismo ejemplar. El índice único también protege la regla.
        using var bloquearLibro = new SqlCommand("SELECT IdLibro FROM dbo.Libros WITH(UPDLOCK,HOLDLOCK) WHERE IdLibro=@libro", conexion, transaccion);
        bloquearLibro.Parameters.Add(P("@libro", SqlDbType.Int, p.IdLibro));
        if (bloquearLibro.ExecuteScalar() is null) throw new InvalidOperationException("El libro seleccionado ya no existe.");
        if (p.Estado == EstadoPrestamo.Prestado)
        {
            using var disponible = new SqlCommand("SELECT COUNT(*) FROM dbo.Prestamos WHERE IdLibro=@libro AND Estado=N'Prestado' AND IdPrestamo<>@id", conexion, transaccion);
            disponible.Parameters.Add(P("@libro", SqlDbType.Int, p.IdLibro));
            disponible.Parameters.Add(P("@id", SqlDbType.Int, p.Id));
            if (Convert.ToInt32(disponible.ExecuteScalar()) > 0)
                throw new InvalidOperationException("Este ejemplar tiene un préstamo activo. Registra primero su devolución.");
        }
        string sql = p.Id == 0
            ? "INSERT dbo.Prestamos(IdUsuario,IdLibro,FechaPrestamo,FechaDevolucion,Estado) VALUES(@usuario,@libro,@fecha,@devolucion,@estado)"
            : "UPDATE dbo.Prestamos SET IdUsuario=@usuario,IdLibro=@libro,FechaPrestamo=@fecha,FechaDevolucion=@devolucion,Estado=@estado WHERE IdPrestamo=@id";
        using var comando = new SqlCommand(sql, conexion, transaccion);
        comando.Parameters.AddRange(new[] { P("@usuario", SqlDbType.Int, p.IdUsuario), P("@libro", SqlDbType.Int, p.IdLibro),
            P("@fecha", SqlDbType.Date, p.FechaPrestamo), P("@devolucion", SqlDbType.Date, p.FechaDevolucion),
            P("@estado", SqlDbType.NVarChar, p.Estado.ToString(), 10), P("@id", SqlDbType.Int, p.Id) });
        if (comando.ExecuteNonQuery() != 1) throw new InvalidOperationException("El préstamo ya no existe.");
        transaccion.Commit();
    }

    public void Devolver(int id, DateTime fecha)
    {
        using var conexion = Abrir();
        using var comando = new SqlCommand(@"UPDATE dbo.Prestamos SET Estado=N'Devuelto',FechaDevolucion=@fecha
            WHERE IdPrestamo=@id AND Estado=N'Prestado' AND FechaPrestamo<=@fecha", conexion);
        comando.Parameters.Add(P("@id", SqlDbType.Int, id));
        comando.Parameters.Add(P("@fecha", SqlDbType.Date, fecha.Date));
        if (comando.ExecuteNonQuery() != 1)
            throw new InvalidOperationException("No se puede devolver: el préstamo no existe, ya fue devuelto o la fecha es anterior al préstamo.");
    }

    public void EliminarPrestamo(int id)
    {
        using var conexion = Abrir();
        using var comando = new SqlCommand("DELETE FROM dbo.Prestamos WHERE IdPrestamo=@id", conexion);
        comando.Parameters.Add(P("@id", SqlDbType.Int, id));
        if (comando.ExecuteNonQuery() != 1) throw new InvalidOperationException("El préstamo ya no existe.");
    }

    public DataTable Resumen() => Consultar(@"
        SELECT N'Libros (ejemplares)' AS Indicador,COUNT(*) AS Total FROM dbo.Libros
        UNION ALL SELECT N'Autores',COUNT(*) FROM dbo.Autores
        UNION ALL SELECT N'Editoriales',COUNT(*) FROM dbo.Editoriales
        UNION ALL SELECT N'Usuarios',COUNT(*) FROM dbo.Usuarios
        UNION ALL SELECT N'Préstamos activos',COUNT(*) FROM dbo.Prestamos WHERE Estado=N'Prestado'
        UNION ALL SELECT N'Préstamos devueltos',COUNT(*) FROM dbo.Prestamos WHERE Estado=N'Devuelto'");
}
