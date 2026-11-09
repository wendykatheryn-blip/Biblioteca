using System.Data;
using System.Net.Mail;
using Biblioteca.Datos;
using Biblioteca.Entidades;

namespace Biblioteca.Negocio;

public sealed class BibliotecaServicio
{
    private readonly BibliotecaRepositorio repositorio;
    public BibliotecaServicio(BibliotecaRepositorio repositorio) { this.repositorio = repositorio; }

    private static string Texto(string? valor, string campo, int longitud, bool obligatorio = true)
    {
        string texto = (valor ?? "").Trim();
        if (obligatorio && texto.Length == 0) throw new ArgumentException($"Completa el campo {campo}.");
        if (texto.Length > longitud) throw new ArgumentException($"{campo} admite máximo {longitud} caracteres.");
        return texto;
    }
    private void Referencia(TipoCatalogo tipo, int id, string campo)
    {
        if (id <= 0 || !repositorio.Existe(tipo, id)) throw new ArgumentException($"Selecciona un {campo} existente.");
    }
    private static void IdValido(int id)
    { if (id <= 0) throw new ArgumentException("Selecciona un registro de la tabla."); }

    public DataTable Listar(TipoCatalogo tipo, string buscar = "") => repositorio.Listar(tipo, Texto(buscar,"Búsqueda",200,false));
    public DataTable Opciones(TipoCatalogo tipo) => repositorio.Opciones(tipo);
    public DataTable Resumen() => repositorio.Resumen();
    public DataTable ListarPrestamos(string buscar = "", string? estado = null) => repositorio.ListarPrestamos(Texto(buscar,"Búsqueda",200,false), estado);

    public void Guardar(Entidad entidad)
    {
        if (entidad.Id < 0) throw new ArgumentException("El identificador no es válido.");
        switch (entidad)
        {
            case Autor a:
                a.Nombre = Texto(a.Nombre,"Nombre",80); a.Apellido = Texto(a.Apellido,"Apellido",80); break;
            case Editorial e:
                e.Nombre = Texto(e.Nombre,"Nombre",120); break;
            case Usuario u:
                u.Documento = Texto(u.Documento,"Documento",30); u.Nombre = Texto(u.Nombre,"Nombre",80);
                u.Apellido = Texto(u.Apellido,"Apellido",80);
                var telefono = Texto(u.Telefono,"Teléfono",30,false);
                var correo = Texto(u.Correo,"Correo",150,false);
                if (correo.Length > 0 && (!MailAddress.TryCreate(correo, out var direccion) || direccion.Address != correo))
                    throw new ArgumentException("Escribe un correo válido, por ejemplo nombre@example.com.");
                u.Telefono = telefono.Length == 0 ? null : telefono;
                u.Correo = correo.Length == 0 ? null : correo; break;
            case Libro l:
                l.Codigo = Texto(l.Codigo,"Código",30); l.Titulo = Texto(l.Titulo,"Título",180);
                Referencia(TipoCatalogo.Autores,l.IdAutor,"autor");
                Referencia(TipoCatalogo.Editoriales,l.IdEditorial,"editorial"); break;
            default: throw new ArgumentException("Tipo de registro no admitido.");
        }
        repositorio.Guardar(entidad);
    }

    public void Eliminar(TipoCatalogo tipo, int id) { IdValido(id); repositorio.Eliminar(tipo,id); }
    public void EliminarPrestamo(int id) { IdValido(id); repositorio.EliminarPrestamo(id); }

    public void GuardarPrestamo(Prestamo p)
    {
        if (p.Id < 0) throw new ArgumentException("Identificador de préstamo no válido.");
        Referencia(TipoCatalogo.Usuarios,p.IdUsuario,"usuario");
        Referencia(TipoCatalogo.Libros,p.IdLibro,"libro");
        if (!Enum.IsDefined(typeof(EstadoPrestamo),p.Estado)) throw new ArgumentException("Estado no válido.");
        if (p.FechaPrestamo.Date > DateTime.Today) throw new ArgumentException("La fecha del préstamo no puede ser futura.");
        if (p.Estado == EstadoPrestamo.Prestado && p.FechaDevolucion.HasValue)
            throw new ArgumentException("Un préstamo activo debe tener la fecha de devolución desmarcada.");
        if (p.Estado == EstadoPrestamo.Devuelto && !p.FechaDevolucion.HasValue)
            throw new ArgumentException("Marca y completa la fecha de devolución.");
        if (p.FechaDevolucion.HasValue && (p.FechaDevolucion.Value.Date < p.FechaPrestamo.Date || p.FechaDevolucion.Value.Date > DateTime.Today))
            throw new ArgumentException("La devolución debe estar entre la fecha del préstamo y hoy.");
        repositorio.GuardarPrestamo(p);
    }

    public void Devolver(int id, DateTime fecha)
    {
        IdValido(id);
        if (fecha.Date > DateTime.Today) throw new ArgumentException("La devolución no puede tener una fecha futura.");
        repositorio.Devolver(id,fecha);
    }

    public static readonly string[] Reportes = { "Resumen", "Libros", "Autores", "Editoriales", "Usuarios",
        "Todos los préstamos", "Préstamos activos", "Préstamos devueltos" };
    public DataTable Reporte(string nombre) => nombre switch
    {
        "Resumen" => Resumen(), "Libros" => Listar(TipoCatalogo.Libros), "Autores" => Listar(TipoCatalogo.Autores),
        "Editoriales" => Listar(TipoCatalogo.Editoriales), "Usuarios" => Listar(TipoCatalogo.Usuarios),
        "Todos los préstamos" => ListarPrestamos(), "Préstamos activos" => ListarPrestamos(estado:"Prestado"),
        "Préstamos devueltos" => ListarPrestamos(estado:"Devuelto"),
        _ => throw new ArgumentException("Selecciona un reporte.")
    };
}
