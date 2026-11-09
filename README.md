# Sistema de gestión de biblioteca

Aplicación de escritorio con C#, Windows Forms y SQL Server. Administra autores, editoriales, usuarios, libros, préstamos, devoluciones y reportes.

**Empieza por [INICIAR_AQUI.md](INICIAR_AQUI.md).**

## Requisitos

- Windows, Visual Studio con Desarrollo de escritorio de .NET y SDK .NET 8, o adaptación del proyecto a un SDK Windows compatible instalado.
- SQL Server Express, Developer o LocalDB, y permisos para crear la base `BibliotecaACA`.
- SSMS para ejecutar el script con separadores GO; DBeaver también puede usarse si está configurado para SQL Server y ejecución de scripts.
- Acceso a NuGet para restaurar Microsoft.Data.SqlClient 6.1.7.
- Git y una cuenta GitHub para publicar el repositorio.

## Ejecución

1. Ejecuta `sql/01_Biblioteca.sql` completo.
2. Abre `Biblioteca.sln`, restaura paquetes y compila.
3. Ejecuta con F5 e ingresa los datos de conexión a esa instancia SQL Server.
4. Revisa el menú y realiza las pruebas de la documentación.

Los formularios se construyen con código C#. No necesitan archivos Designer.cs para funcionar. El código visual está en `Biblioteca/Presentacion/`.

## Estructura

| Ruta | Contenido |
|---|---|
| Biblioteca/Entidades | Clases de los registros y metadatos de los catálogos. |
| Biblioteca/Datos | Repositorio SQL y consultas parametrizadas. |
| Biblioteca/Negocio | Validaciones y coordinación de operaciones. |
| Biblioteca/Presentacion | Conexión, Inicio, catálogos, préstamos y reportes. |
| sql | Creación de la base, datos de prueba y consultas de verificación. |
| docs | Documento técnico y carpeta para capturas reales. |

## Reglas del modelo

- Cada registro de Libros representa un ejemplar con código único.
- Un libro tiene un autor y una editorial en este alcance.
- Cada préstamo relaciona un usuario y un ejemplar. No se necesita DetallePrestamos.
- Un ejemplar admite un solo préstamo activo; una transacción y un índice único protegen esta regla.
- Prestado exige fecha de devolución vacía; Devuelto exige fecha de devolución igual o posterior al préstamo.
- La disponibilidad se calcula a partir de los préstamos activos. No se mantiene un contador de stock separado.
- Se permite editar y eliminar préstamos después de seleccionar y confirmar el registro correspondiente.
- Las claves foráneas impiden eliminar registros relacionados. No hay borrados en cascada.
- El correo y el teléfono son opcionales; si se ingresa correo, debe tener formato válido.

## Publicar con Git

Abre una terminal en esta carpeta y ejecuta `git init` una sola vez:

```bash
git init
git add .
git commit -m "Agregar base de biblioteca y documentación inicial"
```

Después de realizar pruebas y agregar capturas:

```bash
git add docs
git commit -m "Documentar pruebas ejecutadas y agregar capturas"
```

Crea en GitHub un repositorio vacío llamado Biblioteca. Copia su URL real y úsala en lugar de la URL de ejemplo:

```bash
git branch -M main
git remote add origin https://github.com/TU_USUARIO/Biblioteca.git
git push -u origin main
```

Si Git solicita identidad, configura tu nombre y tu correo GitHub con `git config user.name` y `git config user.email`. No publiques contraseñas ni archivos de la base de datos. `.gitignore` excluye compilaciones, archivos locales de Visual Studio y archivos MDF/LDF.

## Evidencias y alcance de validación

Consulta `VALIDACION.md` para el resultado de compilación y las limitaciones verificadas. Las pruebas funcionales con SQL Server, las capturas y la publicación de GitHub se completan en el equipo Windows. Los resultados pendientes se identifican expresamente en la documentación.
