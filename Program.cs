using Microsoft.EntityFrameworkCore;
using PiscinerosAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Conexión a la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Habilitar los controladores
builder.Services.AddControllers();

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