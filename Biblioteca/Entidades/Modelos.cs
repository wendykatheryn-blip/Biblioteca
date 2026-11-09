namespace Biblioteca.Entidades;

// La clase base comparte el identificador de los registros persistentes.
public abstract class Entidad
{
    public int Id { get; set; }
    protected Entidad(int id = 0) { Id = id; }
}

public sealed class Autor : Entidad
{
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string NombreCompleto => Nombre + " " + Apellido;
    public Autor(string nombre, string apellido, int id = 0) : base(id)
    { Nombre = nombre; Apellido = apellido; }
}

public sealed class Editorial : Entidad
{
    public string Nombre { get; set; }
    public Editorial(string nombre, int id = 0) : base(id) { Nombre = nombre; }
}

public sealed class Usuario : Entidad
{
    public string Documento { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public Usuario(string documento, string nombre, string apellido,
        string? telefono, string? correo, int id = 0) : base(id)
    { Documento = documento; Nombre = nombre; Apellido = apellido; Telefono = telefono; Correo = correo; }
}

public sealed class Libro : Entidad
{
    public string Codigo { get; set; }
    public string Titulo { get; set; }
    public int IdAutor { get; set; }
    public int IdEditorial { get; set; }
    public Libro(string codigo, string titulo, int idAutor, int idEditorial, int id = 0) : base(id)
    { Codigo = codigo; Titulo = titulo; IdAutor = idAutor; IdEditorial = idEditorial; }
}

public enum EstadoPrestamo { Prestado, Devuelto }

public sealed class Prestamo : Entidad
{
    public int IdUsuario { get; set; }
    public int IdLibro { get; set; }
    public DateTime FechaPrestamo { get; set; }
    public DateTime? FechaDevolucion { get; set; }
    public EstadoPrestamo Estado { get; set; }
    public Prestamo(int idUsuario, int idLibro, DateTime fechaPrestamo,
        DateTime? fechaDevolucion, EstadoPrestamo estado, int id = 0) : base(id)
    { IdUsuario = idUsuario; IdLibro = idLibro; FechaPrestamo = fechaPrestamo.Date;
      FechaDevolucion = fechaDevolucion?.Date; Estado = estado; }
}

public enum TipoCatalogo { Autores, Editoriales, Usuarios, Libros }

// Metadatos internos para reutilizar el mismo formulario sin duplicar controles.
public sealed record Campo(string Nombre, string Etiqueta, int Longitud = 80,
    bool Obligatorio = true, TipoCatalogo? Referencia = null);

public static class Catalogos
{
    public static string Tabla(TipoCatalogo tipo) => tipo switch
    {
        TipoCatalogo.Autores => "Autores", TipoCatalogo.Editoriales => "Editoriales",
        TipoCatalogo.Usuarios => "Usuarios", TipoCatalogo.Libros => "Libros",
        _ => throw new ArgumentOutOfRangeException(nameof(tipo))
    };
    public static string Clave(TipoCatalogo tipo) => tipo switch
    {
        TipoCatalogo.Autores => "IdAutor", TipoCatalogo.Editoriales => "IdEditorial",
        TipoCatalogo.Usuarios => "IdUsuario", TipoCatalogo.Libros => "IdLibro",
        _ => throw new ArgumentOutOfRangeException(nameof(tipo))
    };
    public static Campo[] Campos(TipoCatalogo tipo) => tipo switch
    {
        TipoCatalogo.Autores => new[] { new Campo("Nombre", "Nombre *"), new Campo("Apellido", "Apellido *") },
        TipoCatalogo.Editoriales => new[] { new Campo("Nombre", "Nombre *", 120) },
        TipoCatalogo.Usuarios => new[] { new Campo("Documento", "Documento *", 30),
            new Campo("Nombre", "Nombre *"), new Campo("Apellido", "Apellido *"),
            new Campo("Telefono", "Teléfono", 30, false), new Campo("Correo", "Correo", 150, false) },
        TipoCatalogo.Libros => new[] { new Campo("Codigo", "Código *", 30), new Campo("Titulo", "Título *", 180),
            new Campo("IdAutor", "Autor *", Referencia: TipoCatalogo.Autores),
            new Campo("IdEditorial", "Editorial *", Referencia: TipoCatalogo.Editoriales) },
        _ => throw new ArgumentOutOfRangeException(nameof(tipo))
    };
}
