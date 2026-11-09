using Biblioteca.Negocio;

namespace Biblioteca.Presentacion;

public sealed class FrmReportes : Form
{
    private readonly BibliotecaServicio servicio;
    private readonly ComboBox reporte=Vista.Lista();
    private readonly DataGridView tabla=Vista.Tabla();
    public FrmReportes(BibliotecaServicio servicio)
    {
        this.servicio=servicio;Vista.Preparar(this,"Reportes");
        var estructura=new TableLayoutPanel { Dock=DockStyle.Fill,ColumnCount=1,RowCount=3 };
        estructura.RowStyles.Add(new RowStyle(SizeType.Absolute,60));estructura.RowStyles.Add(new RowStyle(SizeType.Absolute,55));estructura.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        estructura.Controls.Add(Vista.Titulo("Consultas y reportes"),0,0);
        var opciones=new FlowLayoutPanel { Dock=DockStyle.Fill,Padding=new Padding(12) };
        reporte.Dock=DockStyle.None;reporte.Width=290;reporte.DataSource=BibliotecaServicio.Reportes;
        opciones.Controls.Add(reporte);opciones.Controls.Add(Vista.Boton("Consultar",()=>Vista.Ejecutar(this,Cargar)));
        estructura.Controls.Add(opciones,0,1);estructura.Controls.Add(tabla,0,2);Controls.Add(estructura);
        Shown+=(_,_)=>Vista.Ejecutar(this,Cargar);
    }
    private void Cargar()
    {tabla.DataSource=servicio.Reporte(Convert.ToString(reporte.SelectedItem)??"Resumen");Vista.OcultarReferencias(tabla);tabla.ClearSelection();}
}
