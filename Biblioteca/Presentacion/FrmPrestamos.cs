using Biblioteca.Entidades;
using Biblioteca.Negocio;

namespace Biblioteca.Presentacion;

public sealed class FrmPrestamos : Form
{
    private readonly BibliotecaServicio servicio;
    private readonly ComboBox usuarios=Vista.Lista(),libros=Vista.Lista(),estado=Vista.Lista();
    private readonly DateTimePicker fecha=new() { Format=DateTimePickerFormat.Short,Dock=DockStyle.Fill };
    private readonly DateTimePicker devolucion=new() { Format=DateTimePickerFormat.Short,Dock=DockStyle.Fill,ShowCheckBox=true,Checked=false };
    private readonly TextBox buscar=new() { Width=190,MaxLength=200,PlaceholderText="Usuario, libro o estado" };
    private readonly DataGridView tabla=Vista.Tabla();
    private readonly Label seleccion=new() { Text="Nuevo préstamo",AutoSize=true,Margin=new Padding(14,8,4,4) };
    private int id;
    public FrmPrestamos(BibliotecaServicio servicio)
    {
        this.servicio=servicio;Vista.Preparar(this,"Préstamos y devoluciones",1200,710);
        var estructura=new TableLayoutPanel { Dock=DockStyle.Fill,ColumnCount=1,RowCount=5 };
        estructura.RowStyles.Add(new RowStyle(SizeType.Absolute,60));estructura.RowStyles.Add(new RowStyle(SizeType.Absolute,185));
        estructura.RowStyles.Add(new RowStyle(SizeType.Absolute,55));estructura.RowStyles.Add(new RowStyle(SizeType.Percent,100));estructura.RowStyles.Add(new RowStyle(SizeType.Absolute,42));
        estructura.Controls.Add(Vista.Titulo("Préstamos y devoluciones"),0,0);
        var campos=new TableLayoutPanel { Dock=DockStyle.Fill,Padding=new Padding(12),ColumnCount=4,RowCount=3 };
        campos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,155));campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
        campos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,155));campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
        void Campo(string etiqueta,Control control,int columna,int fila)
        {campos.Controls.Add(new Label { Text=etiqueta,AutoSize=true,Margin=new Padding(4,8,4,4) },columna,fila);control.Margin=new Padding(5);campos.Controls.Add(control,columna+1,fila);}
        Campo("Usuario *",usuarios,0,0);Campo("Libro *",libros,2,0);
        Campo("Fecha préstamo *",fecha,0,1);Campo("Fecha devolución\n(marca si aplica)",devolucion,2,1);
        Campo("Estado *",estado,0,2);estado.DataSource=Enum.GetValues<EstadoPrestamo>();
        estructura.Controls.Add(campos,0,1);
        var acciones=new FlowLayoutPanel { Dock=DockStyle.Fill,Padding=new Padding(8),WrapContents=false,AutoScroll=true };
        acciones.Controls.Add(Vista.Boton("Nuevo",Nuevo));
        acciones.Controls.Add(Vista.Boton("Guardar",()=>Vista.Ejecutar(this,Guardar)));
        acciones.Controls.Add(Vista.Boton("Eliminar",()=>Vista.Ejecutar(this,Eliminar)));
        acciones.Controls.Add(Vista.Boton("Devolver hoy",()=>Vista.Ejecutar(this,Devolver)));
        buscar.Margin=new Padding(12,8,4,4);acciones.Controls.Add(buscar);
        acciones.Controls.Add(Vista.Boton("Buscar",()=>Vista.Ejecutar(this,Cargar)));
        acciones.Controls.Add(Vista.Boton("Actualizar",()=>Vista.Ejecutar(this,Actualizar)));
        estructura.Controls.Add(acciones,0,2);estructura.Controls.Add(tabla,0,3);estructura.Controls.Add(seleccion,0,4);Controls.Add(estructura);
        tabla.CellClick+=(_,e)=>Vista.Ejecutar(this,()=>Seleccionar(e.RowIndex));
        Shown+=(_,_)=>Vista.Ejecutar(this,Actualizar);
    }
    private void Nuevo()
    {id=0;usuarios.SelectedIndex=-1;libros.SelectedIndex=-1;fecha.Value=DateTime.Today;devolucion.Value=DateTime.Today;
     devolucion.Checked=false;estado.SelectedItem=EstadoPrestamo.Prestado;tabla.ClearSelection();seleccion.Text="Nuevo préstamo. Selecciona usuario y libro.";}
    private void Actualizar()
    {Vista.Opciones(usuarios,servicio.Opciones(TipoCatalogo.Usuarios));Vista.Opciones(libros,servicio.Opciones(TipoCatalogo.Libros));buscar.Text="";Cargar();Nuevo();}
    private void Cargar()
    {tabla.DataSource=servicio.ListarPrestamos(buscar.Text);Vista.OcultarReferencias(tabla);tabla.ClearSelection();}
    private void Seleccionar(int indice)
    {
        var fila=Vista.Fila(tabla,indice);if(fila is null)return;
        id=Convert.ToInt32(fila["Id"]);usuarios.SelectedValue=Convert.ToInt32(fila["IdUsuario"]);libros.SelectedValue=Convert.ToInt32(fila["IdLibro"]);
        fecha.Value=Convert.ToDateTime(fila["FechaPrestamo"]);
        devolucion.Value=fila.IsNull("FechaDevolucion")?DateTime.Today:Convert.ToDateTime(fila["FechaDevolucion"]);
        devolucion.Checked=!fila.IsNull("FechaDevolucion");estado.SelectedItem=Enum.Parse<EstadoPrestamo>(Convert.ToString(fila["Estado"])!);
        seleccion.Text=$"Editando préstamo {id}. Devolver hoy registra la devolución con la fecha actual.";
    }
    private void Guardar()
    {
        var prestamo=new Prestamo(Vista.Id(usuarios),Vista.Id(libros),fecha.Value,
            devolucion.Checked?devolucion.Value.Date:null,(EstadoPrestamo)(estado.SelectedItem??EstadoPrestamo.Prestado),id);
        servicio.GuardarPrestamo(prestamo);Cargar();Nuevo();Vista.Correcto(this);
    }
    private void Devolver()
    {Vista.RequerirId(id);servicio.Devolver(id,DateTime.Today);Cargar();Nuevo();Vista.Correcto(this,"Devolución registrada. El libro vuelve a estar disponible.");}
    private void Eliminar()
    {Vista.RequerirId(id);if(!Vista.Confirmar(this))return;servicio.EliminarPrestamo(id);Cargar();Nuevo();Vista.Correcto(this);}
}
