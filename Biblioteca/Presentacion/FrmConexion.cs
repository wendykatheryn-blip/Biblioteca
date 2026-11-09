using Microsoft.Data.SqlClient;
using Biblioteca.Datos;

namespace Biblioteca.Presentacion;

public sealed class FrmConexion : Form
{
    private readonly TextBox servidor=new() { Text=@"(localdb)\MSSQLLocalDB",Dock=DockStyle.Fill,MaxLength=128 };
    private readonly TextBox baseDatos=new() { Text="BibliotecaACA",Dock=DockStyle.Fill,MaxLength=128 };
    private readonly CheckBox windows=new() { Text="Autenticación de Windows",Checked=true,AutoSize=true };
    private readonly TextBox usuario=new() { Dock=DockStyle.Fill,Enabled=false,MaxLength=128 };
    private readonly TextBox clave=new() { Dock=DockStyle.Fill,Enabled=false,UseSystemPasswordChar=true };
    public string CadenaConexion { get; private set; }="";
    public FrmConexion()
    {
        Vista.Preparar(this,"Conectar a SQL Server",660,430);
        var panel=new TableLayoutPanel { Dock=DockStyle.Fill,Padding=new Padding(20),ColumnCount=2,RowCount=8 };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,150));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        var titulo=Vista.Titulo("Conexión a la biblioteca"); panel.Controls.Add(titulo,0,0);panel.SetColumnSpan(titulo,2);
        Agregar(panel,"Servidor",servidor,1); Agregar(panel,"Base de datos",baseDatos,2);
        panel.Controls.Add(windows,1,3); Agregar(panel,"Usuario SQL",usuario,4);Agregar(panel,"Contraseña SQL",clave,5);
        var conectar=Vista.Boton("Conectar y abrir",()=>Vista.Ejecutar(this,Conectar));
        panel.Controls.Add(conectar,1,6); AcceptButton=conectar;
        var ayuda=new Label { AutoSize=true,MaximumSize=new Size(580,0),Text="Primero ejecuta sql/01_Biblioteca.sql. Usa el mismo servidor y autenticación que en SSMS o DBeaver.",Margin=new Padding(8) };
        panel.Controls.Add(ayuda,0,7); panel.SetColumnSpan(ayuda,2);
        windows.CheckedChanged+=(_,_)=>{usuario.Enabled=!windows.Checked;clave.Enabled=!windows.Checked;};
        Controls.Add(panel);
    }
    private static void Agregar(TableLayoutPanel panel,string etiqueta,Control control,int fila)
    { panel.Controls.Add(new Label { Text=etiqueta,AutoSize=true,Margin=new Padding(4,8,4,4) },0,fila);control.Margin=new Padding(4,5,4,8);panel.Controls.Add(control,1,fila); }
    private void Conectar()
    {
        if(string.IsNullOrWhiteSpace(servidor.Text)||string.IsNullOrWhiteSpace(baseDatos.Text))
            throw new ArgumentException("Completa servidor y base de datos.");
        var cadena=new SqlConnectionStringBuilder("Encrypt=True;TrustServerCertificate=True;") {
            DataSource=servidor.Text.Trim(),InitialCatalog=baseDatos.Text.Trim(),
            IntegratedSecurity=windows.Checked,ConnectTimeout=10,ApplicationName="BibliotecaACA" };
        if(!windows.Checked) {cadena.UserID=usuario.Text.Trim();cadena.Password=clave.Text;}
        UseWaitCursor=true;
        try { new BibliotecaRepositorio(cadena.ConnectionString).Resumen(); CadenaConexion=cadena.ConnectionString;DialogResult=DialogResult.OK; }
        finally { UseWaitCursor=false; }
    }
}
