using Microsoft.EntityFrameworkCore;
using PiscinerosAPI.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Conexión a la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Habilitar los controladores
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Esto le dice a C# que si detecta un bucle (Visita -> Tecnico -> Visita), lo ignore y no explote
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // El puerto donde correrá Angular
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

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

app.UseCors("AllowAngular");

// Rutas de la API
app.MapControllers();

app.Run();