using Microsoft.EntityFrameworkCore;
using PiscinerosAPI.Models;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PiscinerosAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Conexión a la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), 
    sqlOptions => sqlOptions.EnableRetryOnFailure()));

// Registro de Servicios Propios
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IImageService, AzureBlobImageService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Activamos Identity con soporte para Usuarios y Roles
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Reglas de las contraseñas
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configuración de JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

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
        var frontendUrl = builder.Configuration["FrontendUrl"];
        
        if (!string.IsNullOrEmpty(frontendUrl))
        {
            // En Producción: Permite solo la URL específica de Azure Static Web Apps
            policy.WithOrigins(frontendUrl)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            // En Desarrollo: Permite la conexión local de Angular
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
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

// Asegurar que exista la carpeta wwwroot/fotos
var fotosPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fotos");
if (!Directory.Exists(fotosPath))
{
    Directory.CreateDirectory(fotosPath);
}

// Configurar para servir la carpeta fotos
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(fotosPath),
    RequestPath = "/fotos"
});

app.UseCors("AllowAngular");

// --- INICIO: Creación de Roles y Usuario Maestro ---
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>> ();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>> ();

    // 1. Crear los roles si no existen
    string[] roles = { "Administrador", "Tecnico" };
    foreach (var rol in roles)
    {
        if (!await roleManager.RoleExistsAsync(rol))
        {
            await roleManager.CreateAsync(new IdentityRole(rol));
        }
    }

    // 2. Crear el usuario administrador inicial
    string adminEmail = "franco@piscineros.cl";
    // Leemos la contraseña desde appsettings o variables de entorno (Azure). Si no existe, usamos una segura temporal.
    string adminPassword = builder.Configuration["AdminPassword"] ?? "REDACTED_ADMIN_PASS"; 

    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);

        if (result.Succeeded)
        {
            // Le asignamos el rol de Administrador
            await userManager.AddToRoleAsync(adminUser, "Administrador");
        }
    }
}
// --- FIN: Creación de Roles y Usuario Maestro ---

app.UseAuthentication();
app.UseAuthorization();

// Rutas de la API (DEBEN ir después de Autenticación y Autorización)
app.MapControllers();

app.Run();