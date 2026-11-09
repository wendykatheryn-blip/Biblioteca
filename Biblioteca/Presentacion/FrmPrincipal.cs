using Biblioteca.Entidades;
using Biblioteca.Negocio;

namespace Biblioteca.Presentacion;

public sealed class FrmPrincipal : Form
{
    private readonly BibliotecaServicio servicio;
    private readonly DataGridView resumen=Vista.Tabla();
    public FrmPrincipal(BibliotecaServicio servicio)
    {
        this.servicio=servicio; Vista.Preparar(this,"Inicio");StartPosition=FormStartPosition.CenterScreen;
        var estructura=new TableLayoutPanel { Dock=DockStyle.Fill,ColumnCount=2,RowCount=1 };
        estructura.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,195));estructura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        var menu=new FlowLayoutPanel { Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,BackColor=Vista.Azul,Padding=new Padding(10) };
        menu.Controls.Add(new Label { Text="BIBLIOTECA",ForeColor=Color.White,AutoSize=true,Font=new Font("Segoe UI",16,FontStyle.Bold),Margin=new Padding(5,14,0,24) });
        void Boton(string texto,Action accion) {var b=Vista.Boton(texto,accion);b.Width=160;b.Height=42;menu.Controls.Add(b);}
        Boton("Inicio",()=>Vista.Ejecutar(this,Actualizar));
        foreach(var tipo in new[]{TipoCatalogo.Autores,TipoCatalogo.Editoriales,TipoCatalogo.Usuarios,TipoCatalogo.Libros})
            Boton(tipo.ToString(),()=>{using var formulario=new FrmCatalogo(tipo,servicio);formulario.ShowDialog(this);Vista.Ejecutar(this,Actualizar);});
        Boton("Préstamos",()=>{using var formulario=new FrmPrestamos(servicio);formulario.ShowDialog(this);Vista.Ejecutar(this,Actualizar);});
        Boton("Reportes",()=>{using var formulario=new FrmReportes(servicio);formulario.ShowDialog(this);});
        Boton("Salir",Close);
        var contenido=new TableLayoutPanel { Dock=DockStyle.Fill,ColumnCount=1,RowCount=3 };
        contenido.RowStyles.Add(new RowStyle(SizeType.Absolute,65));contenido.RowStyles.Add(new RowStyle(SizeType.Absolute,65));contenido.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        contenido.Controls.Add(Vista.Titulo("Gestión de biblioteca"),0,0);
        contenido.Controls.Add(new Label { Text="Registra autores y editoriales, agrega usuarios y libros, y administra los préstamos.\nCada libro representa un ejemplar identificado por un código único.",AutoSize=true,Margin=new Padding(16,5,8,5) },0,1);
        contenido.Controls.Add(resumen,0,2);estructura.Controls.Add(menu,0,0);estructura.Controls.Add(contenido,1,0);Controls.Add(estructura);
        Shown+=(_,_)=>Vista.Ejecutar(this,Actualizar);
    }
    private void Actualizar() {resumen.DataSource=servicio.Resumen();resumen.ClearSelection();}
}
