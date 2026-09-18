using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PiscinerosAPI.Models;
using Microsoft.AspNetCore.Authorization;
using PiscinerosAPI.DTOs;

namespace PiscinerosAPI.Controllers
{
    [Authorize(Roles = "Administrador")]
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/tecnicos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteDto>>> GetClientes()
        {
            return await _context.Clientes
                .AsNoTracking()
                .Select(c => new ClienteDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Direccion = c.Direccion,
                    Comuna = c.Comuna,
                    Telefono = c.Telefono,
                    Correo = c.Correo,
                    VisitasPorMes = c.VisitasPorMes,
                    DiaPreferido = c.DiaPreferido,
                    Observaciones = c.Observaciones
                })
                .ToListAsync();
        }

        // GET: api/tecnicos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteDto>> GetCliente(int id)
        {
            var cliente = await _context.Clientes
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new ClienteDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Direccion = c.Direccion,
                    Comuna = c.Comuna,
                    Telefono = c.Telefono,
                    Correo = c.Correo,
                    VisitasPorMes = c.VisitasPorMes,
                    DiaPreferido = c.DiaPreferido,
                    Observaciones = c.Observaciones
                })
                .FirstOrDefaultAsync();

            if (cliente == null)
            {
                return NotFound();
            }

            return cliente;
        }

        // POST: api/tecnicos
        [HttpPost]
        public async Task<ActionResult<ClienteDto>> PostCliente(ClienteRequestDto dto)
        {
            var cliente = new Cliente
            {
                Nombre = dto.Nombre ?? "",
                Direccion = dto.Direccion ?? "",
                Comuna = dto.Comuna ?? "",
                Telefono = dto.Telefono ?? "",
                Correo = dto.Correo ?? "",
                VisitasPorMes = dto.VisitasPorMes ?? 0,
                DiaPreferido = dto.DiaPreferido ?? "",
                Observaciones = dto.Observaciones ?? ""
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCliente), new { id = cliente.Id }, new { id = cliente.Id });
        }

        // PUT: api/clientes/5
        // Actualiza un cliente existente
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCliente(int id, ClienteRequestDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("El ID de la URL no coincide con el ID del cuerpo de la petición.");
            }

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            if (dto.Nombre != null) cliente.Nombre = dto.Nombre;
            if (dto.Direccion != null) cliente.Direccion = dto.Direccion;
            if (dto.Comuna != null) cliente.Comuna = dto.Comuna;
            if (dto.Telefono != null) cliente.Telefono = dto.Telefono;
            if (dto.Correo != null) cliente.Correo = dto.Correo;
            if (dto.VisitasPorMes != null) cliente.VisitasPorMes = dto.VisitasPorMes.Value;
            if (dto.DiaPreferido != null) cliente.DiaPreferido = dto.DiaPreferido;
            if (dto.Observaciones != null) cliente.Observaciones = dto.Observaciones;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return NoContent();
        }

        // DELETE: api/clientes/5
        // Elimina un cliente de la base de datos
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Función auxiliar para verificar existencia
        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.Id == id);
        }
    }
}
