using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PiscinerosAPI.Models;

namespace PiscinerosAPI.Controllers
{
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

        // DELETE: api/Visitas/5
        // Usar solo para borrar registros creados por error.
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