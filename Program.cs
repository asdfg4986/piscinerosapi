using Microsoft.EntityFrameworkCore;
using PiscinerosAPI.Models;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PiscinerosAPI.Services;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;

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

    // Agregar evento para revocar el token en tiempo real si el técnico es desactivado
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();

            // Extraer el claim de TecnicoId
            var tecnicoIdClaim = context.Principal?.FindFirst("TecnicoId")?.Value;
            if (!string.IsNullOrEmpty(tecnicoIdClaim) && int.TryParse(tecnicoIdClaim, out int tecnicoId))
            {
                // Buscar al técnico en la base de datos para ver si sigue activo
                var tecnico = await dbContext.Tecnicos.FindAsync(tecnicoId);
                if (tecnico == null || !tecnico.Activo)
                {
                    // Si el técnico fue eliminado o desactivado, invalidamos su token inmediatamente
                    context.Fail("El técnico ha sido desactivado y su acceso fue revocado.");
                }
            }
        }
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
// Configuración de ForwardedHeaders para leer la IP real del cliente detrás del proxy de Azure App Service
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Configuración de Rate Limiting (Protección contra Fuerza Bruta)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Política para Login: máximo 5 intentos por minuto por IP
    options.AddPolicy("login-policy", httpContext =>
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: clientIp,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });

    // Política para Health Check: máximo 6 peticiones por minuto por IP (UptimeRobot solo necesita 1 cada 5 o 10 min)
    options.AddPolicy("health-policy", httpContext =>
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: clientIp,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 6,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            System.Text.Json.JsonSerializer.Serialize(new
            {
                mensaje = "Demasiadas solicitudes. Por favor, espera un minuto antes de volver a intentar."
            }), cancellationToken: token);
    };
});

builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseForwardedHeaders();

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
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // Ejecuta las migraciones pendientes automáticamente en la base de datos (ideal para despliegue en Azure)
    dbContext.Database.Migrate();

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

    // 2. Crear los usuarios administradores iniciales
    // Leemos la contraseña desde appsettings o variables de entorno (Azure). Si no existe, usamos una segura temporal.
    string adminPassword = builder.Configuration["AdminPassword"] ?? "CAMBIAME_EN_PRODUCCION_123!"; 
    
    string[] adminEmails = { "franco@piscineros.cl", "erwin@piscineros.cl" };

    foreach (var email in adminEmails)
    {
        if (await userManager.FindByEmailAsync(email) == null)
        {
            var adminUser = new IdentityUser
            {
                UserName = email,
                Email = email
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                // Le asignamos el rol de Administrador
                await userManager.AddToRoleAsync(adminUser, "Administrador");
            }
        }
    }
}
// --- FIN: Creación de Roles y Usuario Maestro ---

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Rutas de la API (DEBEN ir después de Autenticación y Autorización)
app.MapControllers();

app.Run();
