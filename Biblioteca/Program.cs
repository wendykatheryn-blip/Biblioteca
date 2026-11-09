using Biblioteca.Datos;
using Biblioteca.Negocio;
using Biblioteca.Presentacion;

namespace Biblioteca;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        using var conexion = new FrmConexion();
        if (conexion.ShowDialog() != DialogResult.OK) return;
        var repositorio = new BibliotecaRepositorio(conexion.CadenaConexion);
        Application.Run(new FrmPrincipal(new BibliotecaServicio(repositorio)));
    }
}
