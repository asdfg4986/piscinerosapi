using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace PiscinerosAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ConfiguracionController : ControllerBase
    {
        [HttpGet("comunas")]
        public IActionResult GetComunas()
        {
            // Lista inicial de comunas
            var comunas = new List<object>
            {
                new { id = 1, nombre = "Buin" },
                new { id = 2, nombre = "Calera de Tango" },
                new { id = 3, nombre = "Cerrillos" },
                new { id = 4, nombre = "Cerro Navia" },
                new { id = 5, nombre = "Colina" },
                new { id = 6, nombre = "El Bosque" },
                new { id = 7, nombre = "Estación Central" },
                new { id = 8, nombre = "Huechuraba" },
                new { id = 9, nombre = "Independencia" },
                new { id = 10, nombre = "Isla de Maipo" },
                new { id = 11, nombre = "La Cisterna" },
                new { id = 12, nombre = "La Florida" },
                new { id = 13, nombre = "La Granja" },
                new { id = 14, nombre = "La Pintana" },
                new { id = 15, nombre = "La Reina" },
                new { id = 16, nombre = "Lampa" },
                new { id = 17, nombre = "Las Condes" },
                new { id = 18, nombre = "Lo Barnechea" },
                new { id = 19, nombre = "Lo Espejo" },
                new { id = 20, nombre = "Lo Prado" },
                new { id = 21, nombre = "Macul" },
                new { id = 22, nombre = "Maipú" },
                new { id = 23, nombre = "Ñuñoa" },
                new { id = 24, nombre = "Padre Hurtado" },
                new { id = 25, nombre = "Paine" },
                new { id = 26, nombre = "Pedro Aguirre Cerda" },
                new { id = 27, nombre = "Peñaflor" },
                new { id = 28, nombre = "Peñalolén" },
                new { id = 29, nombre = "Pirque" },
                new { id = 30, nombre = "Providencia" },
                new { id = 31, nombre = "Pudahuel" },
                new { id = 32, nombre = "Puente Alto" },
                new { id = 33, nombre = "Quilicura" },
                new { id = 34, nombre = "Quinta Normal" },
                new { id = 35, nombre = "Recoleta" },
                new { id = 36, nombre = "Renca" },
                new { id = 37, nombre = "San Bernardo" },
                new { id = 38, nombre = "San Joaquín" },
                new { id = 39, nombre = "San Miguel" },
                new { id = 40, nombre = "San Ramón" },
                new { id = 41, nombre = "Santiago" },
                new { id = 42, nombre = "Talagante" },
                new { id = 43, nombre = "Vitacura" }
            };

            return Ok(comunas);
        }
    }
}