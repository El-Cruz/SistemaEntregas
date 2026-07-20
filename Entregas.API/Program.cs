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
    app.UseDefaultFiles();
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

app.Run();
      