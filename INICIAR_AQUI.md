# Pasos para ejecutar y terminar la entrega

1. Descomprime TODO el ZIP en una carpeta de tu equipo, por ejemplo `Documentos/Biblioteca_Entrega`. No abras la solución dentro del ZIP.
2. En SSMS, conéctate al servidor SQL que ya tienes funcionando. Anota el servidor y la autenticación utilizada.
3. Abre `sql/01_Biblioteca.sql` con SSMS y ejecuta el archivo completo con F5. En DBeaver, usa la opción Ejecutar script SQL; si tu configuración no interpreta GO, utiliza SSMS para este archivo.
4. Al finalizar deben aparecer las cinco tablas. En una base nueva verás: Autores 2, Editoriales 2, Usuarios 2, Libros 3 y Prestamos 2. El script no borra registros y puede ejecutarse otra vez.
5. Abre `Biblioteca.sln` con Visual Studio. El proyecto ya está creado; no necesitas crear otro ni copiar clases manualmente.
6. Espera la restauración de paquetes NuGet. El proyecto usa .NET 8 y Microsoft.Data.SqlClient 6.1.7. Si Visual Studio indica que falta el SDK .NET 8, agrégalo en Visual Studio Installer. Si ya trabajas con un SDK posterior, puedes ajustar TargetFramework en el `.csproj` a la versión Windows que tengas instalada.
7. Selecciona Compilar > Compilar solución. Después ejecuta con F5.
8. Aparecerá una ventana de conexión. Cambia Servidor por EXACTAMENTE el que usaste en SSMS. La base debe ser `BibliotecaACA`. El valor inicial `(localdb)\MSSQLLocalDB` solo funciona si tienes LocalDB instalado. Otro ejemplo de instancia Express es `.\SQLEXPRESS`; usa tu nombre real.
9. Si entraste a SSMS con Windows, deja marcada Autenticación de Windows. Si usas usuario SQL, desmárcala e ingresa el mismo usuario y contraseña. La aplicación mantiene la cadena en memoria y no guarda la contraseña en el repositorio.
10. Pulsa Conectar y abrir. Debe aparecer Inicio con el resumen.
11. Revisa Autores, Editoriales, Usuarios y Libros. Para editar, haz clic en una fila y luego Guardar. Para crear otro registro, pulsa Nuevo.
12. Abre Préstamos. Pulsa Nuevo. Selecciona Laura Gómez, el libro BIB-003, la fecha actual y estado Prestado. La fecha de devolución debe estar DESMARCADA. Guarda y toma una captura.
13. Selecciona ese préstamo y pulsa Devolver hoy. Debe quedar Devuelto y con fecha de devolución. Toma una captura.
14. Para registrar una devolución de otro día, selecciona el préstamo, cambia Estado a Devuelto, marca la fecha de devolución, elige una fecha entre el préstamo y hoy y pulsa Guardar.
15. Abre Reportes. Consulta préstamos activos y devueltos. Toma capturas.
16. Completa las pruebas de `docs/Documentacion.md`. Solo marca Aprobado después de ejecutar cada caso.
17. Guarda las capturas reales en `docs/capturas/` con los nombres indicados en la documentación.
18. Completa el nombre del docente, las evidencias y los resultados en `docs/Documentacion.md`.
19. Crea el repositorio en GitHub y sube el contenido del proyecto con Git, siguiendo README.md. Haz commits conforme avances; no inventes actividad anterior.
20. Entrega la URL del repositorio y los archivos que solicite la plataforma. Mantén la documentación `.md`. La guía también pide páginas numeradas: adjunta una copia PDF paginada de la documentación si el docente aplica ese requisito.

## Si aparece un error

| Mensaje | Acción |
|---|---|
| No se encuentra el servidor | Revisa el nombre de la instancia y que SQL Server esté iniciado. Copia la configuración que funciona en SSMS. |
| Cannot open database BibliotecaACA | Ejecuta primero `sql/01_Biblioteca.sql` en esa misma instancia. |
| Login failed | Revisa si debes usar Windows o usuario SQL. |
| No puede restaurar Microsoft.Data.SqlClient | Revisa Internet y que nuget.org esté habilitado en las fuentes de paquetes de Visual Studio. |
| Ya existe un registro | Cambia el código/documento/nombre único o devuelve el libro que ya tiene un préstamo activo. |
| No se puede eliminar un autor, editorial, usuario o libro | Está relacionado con otros registros. Las claves foráneas impiden dejar relaciones incompletas. |

## Qué hace el script

Crea una base nueva llamada BibliotecaACA si no existe, crea las tablas faltantes e inserta datos de prueba que todavía no existan. No es un script de migración para esquemas anteriores diferentes; utiliza esta base dedicada al ejercicio.
