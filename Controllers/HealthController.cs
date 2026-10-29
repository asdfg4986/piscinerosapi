using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace PiscinerosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    [EnableRateLimiting("health-policy")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Endpoint ultraligero de Keep-Alive para UptimeRobot / comprobación de estado.
        /// Protegido con Rate Limiting (máximo 6 consultas por minuto por IP).
        /// No consulta la base de datos ni requiere autenticación JWT.
        /// </summary>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
