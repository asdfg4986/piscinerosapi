using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PiscinerosAPI.DTOs;

namespace PiscinerosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly PiscinerosAPI.Models.ApplicationDbContext _context;

        // Inyectamos el UserManager (para revisar la BD), la Configuración y el Contexto (para buscar al técnico)
        public AuthController(UserManager<IdentityUser> userManager, IConfiguration configuration, PiscinerosAPI.Models.ApplicationDbContext context)
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            // 1. Buscamos si existe un usuario con ese correo
            var user = await _userManager.FindByEmailAsync(model.Email);

            // 2. Verificamos que el usuario exista y que la contraseña coincida
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                // 3. Obtenemos los roles del usuario (ej. "Administrador" o "Tecnico")
                var userRoles = await _userManager.GetRolesAsync(user);

                // 4. Creamos los "Claims" (Datos que irán dentro de la pulsera VIP)
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id), // El ID interno del usuario
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Un ID único para este token
                };

                // Si es un técnico, buscamos su ID en la tabla Tecnicos y lo agregamos
                if (userRoles.Contains("Tecnico"))
                {
                    var tecnico = _context.Tecnicos.FirstOrDefault(t => t.IdentityUserId == user.Id);
                    if (tecnico != null)
                    {
                        authClaims.Add(new Claim("TecnicoId", tecnico.Id.ToString()));
                    }
                }

                // Agregamos cada rol que tenga el usuario al Token
                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                // 5. Preparamos la llave criptográfica
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

                // 6. Construimos el Token JWT
                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    expires: DateTime.Now.AddHours(10), // El token durará 10 horas
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                // 7. Devolvemos el Token al frontend (Angular)
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo,
                    roles = userRoles // Enviamos los roles por separado por si Angular quiere mostrarlos u ocultar botones
                });
            }

            // Si falla el correo o la contraseña, devolvemos un error 401 Unauthorized
            return Unauthorized(new { mensaje = "Correo o contraseña incorrectos." });
        }
    }
}
