using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PiscinerosAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace PiscinerosAPI.Controllers
{
    [Authorize(Roles = "Administrador, Tecnico")]
    [Route("api/[controller]")]
    [ApiController]
    public class VisitasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VisitasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Visitas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Visita>>> GetVisitas()
        {
            // Traemos las visitas incluyendo los datos del Cliente y Técnico
            return await _context.Visitas
                .Include(v => v.Cliente)
                .Include(v => v.Tecnico)
                .ToListAsync();
        }

        // GET: api/Visitas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Visita>> GetVisita(int id)
        {
            var visita = await _context.Visitas
                .Include(v => v.Cliente)
                .Include(v => v.Tecnico)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visita == null)
            {
                return NotFound();
            }

            return visita;
        }


        // POST: api/Visitas
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<ActionResult<Visita>> PostVisita(Visita visita)
        {
            _context.Visitas.Add(visita);
            await _context.SaveChangesAsync();

            // Usamos nameof(GetVisita) para que apunte correctamente al método de arriba
            return CreatedAtAction(nameof(GetVisita), new { id = visita.Id }, visita);
        }

        // PUT: api/Visitas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVisita(int id, Visita visita)
        {
            if (id != visita.Id)
            {
                return BadRequest("El ID de la ruta no coincide con el ID del cuerpo (JSON).");
            }

            _context.Entry(visita).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Visitas.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // GET: api/Visitas/tecnico/5/hoy
        // Devuelve solo las visitas asignadas a un técnico específico para el día de hoy
        [HttpGet("tecnico/{tecnicoId}/hoy")]
        public async Task<ActionResult<IEnumerable<Visita>>> GetRutaDiaria(int tecnicoId)
        {
            // Obtenemos la fecha de hoy a las 00:00:00
            var hoy = DateTime.Today;

            var rutaDiaria = await _context.Visitas
                .Include(v => v.Cliente)
                .Where(v => v.TecnicoId == tecnicoId && v.FechaVisita.Date == hoy)
                .OrderBy(v => v.FechaVisita) // Las ordenamos cronológicamente
                .ToListAsync();

            if (!rutaDiaria.Any())
            {
                // Si no tiene visitas hoy, devolvemos un 200 OK con una lista vacía.
                return Ok(new List<Visita>());
            }

            return Ok(rutaDiaria);
        }

        // GET: api/Visitas/cliente/5
        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<Visita>>> GetVisitasPorCliente(int clienteId)
        {
            // Buscamos en la tabla Visitas todas las que coincidan con el ClienteId
            var visitas = await _context.Visitas
                                        .Include (v => v.Tecnico)
                                        .Where(v => v.ClienteId == clienteId)
                                        .OrderByDescending(v => v.FechaVisita) // Ordenamos para que la más reciente salga primero
                                        .ToListAsync();

            // Aunque no tenga visitas, devolvemos un 200 OK con una lista vacía.
            return Ok(visitas);
        }

        // DELETE: api/Visitas/5
        // Usar solo para borrar registros creados por error.
        [Authorize(Roles = "Administrador")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVisita(int id)
        {
            var visita = await _context.Visitas.FindAsync(id);
            if (visita == null)
            {
                return NotFound();
            }

            _context.Visitas.Remove(visita);
            await _context.SaveChangesAsync();

            return NoContent(); // Retorna un código 204 (Éxito, pero sin contenido que devolver)
        }
    }
}