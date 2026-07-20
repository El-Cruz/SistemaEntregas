using Entregas.API.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar la Base de Datos
// Utilizaremos una base de datos local temporal para la API por el momento
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=entregas_backend.db"));

// 2. Agregar soporte para Controladores
builder.Services.AddControllers();

// 3. Configurar Swagger (para probar la API visualmente)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4. Configurar el entorno de peticiones
if (app.Environment.IsDevelopment())
{
    app.UseDefaultFiles(new DefaultFilesOptions
    {
        DefaultFileNames = new List<string> { "index.html" }
    });
    app.UseStaticFiles();
}

app.UseAuthorization();
app.MapControllers(); // Mapear las rutas de nuestros controladores

// Aplicar migraciones y crear la base de datos automáticamente
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>(); // Cambiado a AppDbcontext.Database.EnsureCreated();
}

app.MapGet("/", async (IWebHostEnvironment env) =>
{
    // Construimos la ruta exacta dentro del contenedor de Linux
    var filePath = Path.Combine(env.ContentRootPath, "wwwroot", "index.html");

    // Verificamos si el archivo realmente llegó al servidor
    if (System.IO.File.Exists(filePath))
    {
        var html = await System.IO.File.ReadAllTextAsync(filePath);
        return Results.Content(html, "text/html");
    }

    // Si falla, nos mostrará exactamente dónde está buscando
    return Results.Content(
        $"<h1>Error 404 Diagnosticado</h1>" +
        $"<p>La API está funcionando, pero el archivo HTML no se encontró.</p>" +
        $"<p>Ruta donde se buscó: <b>{filePath}</b></p>",
        "text/html"
    );
});
// Aplicar migraciones automáticamente al arrancar
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>(); // Cambia TuDbContext por tu clase
        context.Database.Migrate(); // Esto crea las tablas usando tus migraciones de Entity Framework
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al migrar la base de datos.");
    }
}

app.Run();

app.Run();
      