using Biblioteca.Entidades;
using Biblioteca.Negocio;

namespace Biblioteca.Presentacion;

public sealed class FrmCatalogo : Form
{
    private readonly TipoCatalogo tipo;
    private readonly BibliotecaServicio servicio;
    private readonly Dictionary<string,Control> controles=new();
    private readonly DataGridView tabla=Vista.Tabla();
    private readonly TextBox buscar=new() { Width=240,MaxLength=200,PlaceholderText="Código, título o nombre" };
    private readonly Label seleccion=new() { Text="Nuevo registro",AutoSize=true,Margin=new Padding(14,8,4,4) };
    private int id;
    public FrmCatalogo(TipoCatalogo tipo,BibliotecaServicio servicio)
    {
        this.tipo=tipo;this.servicio=servicio;Vista.Preparar(this,tipo.ToString());
        var estructura=new TableLayoutPanel { Dock=DockStyle.Fill,ColumnCount=1,RowCount=5 };
        estructura.RowStyles.Add(new RowStyle(SizeType.Absolute,60));estructura.RowStyles.Add(new RowStyle(SizeType.Absolute,175));
        estructura.RowStyles.Add(new RowStyle(SizeType.Absolute,55));estructura.RowStyles.Add(new RowStyle(SizeType.Percent,100));estructura.RowStyles.Add(new RowStyle(SizeType.Absolute,40));
        estructura.Controls.Add(Vista.Titulo(tipo.ToString()),0,0);
        var campos=new TableLayoutPanel { Dock=DockStyle.Fill,ColumnCount=4,Padding=new Padding(12),RowCount=3 };
        campos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,115));campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
        campos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,115));campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
        var definicion=Catalogos.Campos(tipo);
        for(int i=0;i<definicion.Length;i++)
        {
            var campo=definicion[i];int columna=(i%2)*2;int fila=i/2;
            Control control=campo.Referencia.HasValue ? Vista.Lista() : new TextBox { Dock=DockStyle.Fill,MaxLength=campo.Longitud };
            control.Margin=new Padding(5);controles[campo.Nombre]=control;
            campos.Controls.Add(new Label { Text=campo.Etiqueta,AutoSize=true,Margin=new Padding(4,9,4,4) },columna,fila);
            campos.Controls.Add(control,columna+1,fila);
        }
        estructura.Controls.Add(campos,0,1);
        var acciones=new FlowLayoutPanel { Dock=DockStyle.Fill,Padding=new Padding(8),WrapContents=false,AutoScroll=true };
        acciones.Controls.Add(Vista.Boton("Nuevo",Nuevo));
        acciones.Controls.Add(Vista.Boton("Guardar",()=>Vista.Ejecutar(this,Guardar)));
        acciones.Controls.Add(Vista.Boton("Eliminar",()=>Vista.Ejecutar(this,Eliminar)));
        buscar.Margin=new Padding(12,8,4,4);acciones.Controls.Add(buscar);
        acciones.Controls.Add(Vista.Boton("Buscar",()=>Vista.Ejecutar(this,Cargar)));
        acciones.Controls.Add(Vista.Boton("Actualizar",()=>Vista.Ejecutar(this,Actualizar)));
        estructura.Controls.Add(acciones,0,2);estructura.Controls.Add(tabla,0,3);estructura.Controls.Add(seleccion,0,4);Controls.Add(estructura);
        tabla.CellClick+=(_,e)=>Vista.Ejecutar(this,()=>Seleccionar(e.RowIndex));
        Shown+=(_,_)=>Vista.Ejecutar(this,Actualizar);
    }
    private string Texto(string campo)=>controles[campo].Text.Trim();
    private int Referencia(string campo)=>Vista.Id((ComboBox)controles[campo]);
    private void Nuevo()
    {
        id=0;foreach(var control in controles.Values) {if(control is ComboBox lista) lista.SelectedIndex=-1;else control.Text="";}
        seleccion.Text="Nuevo registro. Los campos con * son obligatorios.";tabla.ClearSelection();
    }
    private void Actualizar()
    {
        foreach(var campo in Catalogos.Campos(tipo))
            if(campo.Referencia.HasValue) Vista.Opciones((ComboBox)controles[campo.Nombre],servicio.Opciones(campo.Referencia.Value));
        buscar.Text="";Cargar();Nuevo();
    }
    private void Cargar() {tabla.DataSource=servicio.Listar(tipo,buscar.Text);Vista.OcultarReferencias(tabla);tabla.ClearSelection();}
    private void Seleccionar(int indice)
    {
        var fila=Vista.Fila(tabla,indice);if(fila is null)return;id=Convert.ToInt32(fila["Id"]);
        foreach(var campo in Catalogos.Campos(tipo))
            if(controles[campo.Nombre] is ComboBox lista) lista.SelectedValue=Convert.ToInt32(fila[campo.Nombre]);
            else controles[campo.Nombre].Text=Convert.ToString(fila[campo.Nombre])??"";
        seleccion.Text=$"Editando registro {id}. Pulsa Nuevo para crear otro.";
    }
    private void Guardar()
    {
        Entidad entidad=tipo switch {
            TipoCatalogo.Autores=>new Autor(Texto("Nombre"),Texto("Apellido"),id),
            TipoCatalogo.Editoriales=>new Editorial(Texto("Nombre"),id),
            TipoCatalogo.Usuarios=>new Usuario(Texto("Documento"),Texto("Nombre"),Texto("Apellido"),Texto("Telefono"),Texto("Correo"),id),
            TipoCatalogo.Libros=>new Libro(Texto("Codigo"),Texto("Titulo"),Referencia("IdAutor"),Referencia("IdEditorial"),id),
            _=>throw new ArgumentException("Catálogo no válido.") };
        servicio.Guardar(entidad);Cargar();Nuevo();Vista.Correcto(this);
    }
    private void Eliminar() {Vista.RequerirId(id);if(!Vista.Confirmar(this))return;servicio.Eliminar(tipo,id);Cargar();Nuevo();Vista.Correcto(this);}
}
