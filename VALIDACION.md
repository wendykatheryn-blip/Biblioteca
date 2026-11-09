# Validación del paquete

- Compilación realizada con SDK .NET 8.0.427, destino net8.0-windows y Windows Forms.
- Microsoft.Data.SqlClient 6.1.7 restaurado correctamente desde NuGet.
- Resultado de `dotnet build -p:EnableWindowsTargeting=true`: 0 errores y 0 advertencias.
- El parámetro EnableWindowsTargeting se usó para compilar desde Linux. No cambia el destino de ejecución: la aplicación se ejecuta en Windows.
- No se ejecutó la interfaz Windows Forms ni el script contra una instancia SQL Server en este entorno. Las pruebas funcionales y las capturas quedan pendientes en el equipo Windows del estudiante.
- El repositorio remoto GitHub no se ha creado ni publicado. README.md incluye las instrucciones para hacerlo.
- El documento técnico corresponde a los archivos y al esquema incluidos. Tiene pendientes explícitos para docente, resultados, capturas y URL del repositorio.
- El paquete contiene código fuente; no incluye bin/obj ni un ejecutable publicado. Visual Studio debe restaurar los paquetes y compilar.
