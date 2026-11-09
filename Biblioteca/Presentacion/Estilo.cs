using System.Data;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Presentacion;

internal static class Vista
{
    public static readonly Color Azul = Color.FromArgb(30, 64, 109);
    public static void Preparar(Form formulario, string titulo, int ancho = 1120, int alto = 690)
    {
        formulario.Text = "Biblioteca | " + titulo;
        formulario.ClientSize = new Size(ancho,alto);
        formulario.MinimumSize = new Size(Math.Min(960,ancho),Math.Min(630,alto));
        formulario.StartPosition = FormStartPosition.CenterParent;
        formulario.Font = new Font("Segoe UI",10);
        formulario.BackColor = Color.FromArgb(245,247,251);
    }
    public static Label Titulo(string texto) => new Label { Text=texto, AutoSize=true,
        Font=new Font("Segoe UI",19,FontStyle.Bold), ForeColor=Azul, Margin=new Padding(12,10,6,4) };
    public static Button Boton(string texto, Action accion)
    {
        var boton=new Button { Text=texto,AutoSize=true,Height=34,Padding=new Padding(10,3,10,3),Margin=new Padding(4),BackColor=Color.White };
        boton.Click+=(_,_)=>accion(); return boton;
    }
    public static DataGridView Tabla() => new DataGridView { Dock=DockStyle.Fill,ReadOnly=true,
        AllowUserToAddRows=false,AllowUserToDeleteRows=false,MultiSelect=false,
        SelectionMode=DataGridViewSelectionMode.FullRowSelect,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,
        BackgroundColor=Color.White,BorderStyle=BorderStyle.None,RowHeadersVisible=false,
        AutoSizeRowsMode=DataGridViewAutoSizeRowsMode.AllCells,Margin=new Padding(12) };
    public static ComboBox Lista() => new ComboBox { Dock=DockStyle.Fill,DropDownStyle=ComboBoxStyle.DropDownList };
    public static void Opciones(ComboBox lista,DataTable datos)
    { lista.DisplayMember="Descripcion"; lista.ValueMember="Id"; lista.DataSource=datos; lista.SelectedIndex=-1; }
    public static int Id(ComboBox lista) => lista.SelectedValue is int valor ? valor : 0;
    public static void OcultarReferencias(DataGridView tabla)
    { foreach(string columna in new[]{"IdAutor","IdEditorial","IdUsuario","IdLibro"})
        if(tabla.Columns.Contains(columna)) tabla.Columns[columna].Visible=false; }
    public static DataRow? Fila(DataGridView tabla,int indice) => indice>=0 && tabla.Rows[indice].DataBoundItem is DataRowView fila ? fila.Row : null;
    public static void Ejecutar(IWin32Window ventana,Action accion)
    {
        try { accion(); }
        catch(SqlException error)
        {
            string mensaje=error.Number switch {
                2601 or 2627=>"Ya existe un registro con ese código, documento o nombre único, o un préstamo activo para el libro.",
                547=>"La operación incumple una relación o validación. No puedes eliminar registros asociados a libros o préstamos.",
                -2=>"La conexión tardó demasiado. Comprueba que SQL Server esté disponible.",
                _=>"No se pudo completar la operación en SQL Server. " + error.Message };
            MessageBox.Show(ventana,mensaje,"Revisa la información",MessageBoxButtons.OK,MessageBoxIcon.Warning);
        }
        catch(Exception error) { MessageBox.Show(ventana,error.Message,"Revisa la información",MessageBoxButtons.OK,MessageBoxIcon.Warning); }
    }
    public static void Correcto(IWin32Window ventana,string texto="Operación realizada correctamente.") =>
        MessageBox.Show(ventana,texto,"Biblioteca",MessageBoxButtons.OK,MessageBoxIcon.Information);
    public static bool Confirmar(IWin32Window ventana) => MessageBox.Show(ventana,
        "Se eliminará el registro seleccionado. ¿Continuar?","Confirmar eliminación",
        MessageBoxButtons.YesNo,MessageBoxIcon.Question)==DialogResult.Yes;
    public static void RequerirId(int id)
    { if(id<=0) throw new ArgumentException("Haz clic en un registro de la tabla."); }
}
