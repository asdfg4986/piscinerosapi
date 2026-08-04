using Microsoft.EntityFrameworkCore;
using PiscinerosAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Conexión a la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Habilitar los controladores
builder.Services.AddControllers();

// 1. ESTO REEMPLAZA A "AddOpenApi()" EN .NET 8
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 2. ESTO REEMPLAZA A "MapOpenApi()" Y DIBUJA LA INTERFAZ GRÁFICA
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Rutas de la API
app.MapControllers();

app.Run();