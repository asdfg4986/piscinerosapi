using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PiscinerosAPI.Models;
using Microsoft.AspNetCore.Authorization;
using PiscinerosAPI.DTOs;
using System.Security.Claims;

namespace PiscinerosAPI.Controllers
{
    [Authorize(Roles = "Administrador, Tecnico")]
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/clientes
        [Authorize(Roles = "Administrador, Tecnico")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteDto>>> GetClientes()
        {
            var query = _context.Clientes.Include(c => c.TecnicoExterno).AsNoTracking();

            if (User.IsInRole("Tecnico"))
            {
                var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var tecnico = await _context.Tecnicos.FirstOrDefaultAsync(t => t.IdentityUserId == identityUserId);

                if (tecnico == null || !tecnico.EsExterno)
                {
                    return Forbid();
                }

                query = query.Where(c => c.TecnicoExternoId == tecnico.Id);
            }

            return await query
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
                    Observaciones = c.Observaciones,
                    Activo = c.Activo,
                    NumeroClienteLegacy = c.NumeroClienteLegacy,
                    TecnicoExternoId = c.TecnicoExternoId,
                    NombreTecnicoExterno = c.TecnicoExterno != null ? c.TecnicoExterno.Nombre : null
                })
                .ToListAsync();
        }

        // GET: api/clientes/5
        [Authorize(Roles = "Administrador, Tecnico")]
        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteDto>> GetCliente(int id)
        {
            var query = _context.Clientes.Include(c => c.TecnicoExterno).AsNoTracking().Where(c => c.Id == id);

            if (User.IsInRole("Tecnico"))
            {
                var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var tecnico = await _context.Tecnicos.FirstOrDefaultAsync(t => t.IdentityUserId == identityUserId);

                if (tecnico == null || !tecnico.EsExterno)
                {
                    return Forbid();
                }

                query = query.Where(c => c.TecnicoExternoId == tecnico.Id);
            }

            var cliente = await query
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
                    Observaciones = c.Observaciones,
                    Activo = c.Activo,
                    NumeroClienteLegacy = c.NumeroClienteLegacy,
                    TecnicoExternoId = c.TecnicoExternoId,
                    NombreTecnicoExterno = c.TecnicoExterno != null ? c.TecnicoExterno.Nombre : null
                })
                .FirstOrDefaultAsync();

            if (cliente == null)
            {
                return NotFound();
            }

            return cliente;
        }

        // POST: api/tecnicos
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<ActionResult<ClienteDto>> PostCliente(ClienteRequestDto dto)
        {
            if (!string.IsNullOrEmpty(dto.NumeroClienteLegacy))
            {
                var existe = await _context.Clientes.AnyAsync(c => c.NumeroClienteLegacy == dto.NumeroClienteLegacy);
                if (existe) return BadRequest(new { mensaje = "El ID Heredado ya está en uso." });
            }

            var cliente = new Cliente
            {
                Nombre = dto.Nombre ?? "",
                Direccion = dto.Direccion ?? "",
                Comuna = dto.Comuna ?? "",
                Telefono = dto.Telefono ?? "",
                Correo = dto.Correo ?? "",
                VisitasPorMes = dto.VisitasPorMes ?? 0,
                DiaPreferido = dto.DiaPreferido ?? "",
                Observaciones = dto.Observaciones ?? "",
                Activo = dto.Activo ?? true,
                NumeroClienteLegacy = dto.NumeroClienteLegacy,
                TecnicoExternoId = dto.TecnicoExternoId
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCliente), new { id = cliente.Id }, new { id = cliente.Id });
        }

        // PUT: api/clientes/5
        // Actualiza un cliente existente
        [Authorize(Roles = "Administrador")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCliente(int id, ClienteRequestDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("El ID de la URL no coincide con el ID del cuerpo de la petición.");
            }

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            if (!string.IsNullOrEmpty(dto.NumeroClienteLegacy) && dto.NumeroClienteLegacy != cliente.NumeroClienteLegacy)
            {
                var existe = await _context.Clientes.AnyAsync(c => c.NumeroClienteLegacy == dto.NumeroClienteLegacy && c.Id != id);
                if (existe) return BadRequest(new { mensaje = "El ID Heredado ya está en uso por otro cliente." });
            }

            if (dto.Nombre != null) cliente.Nombre = dto.Nombre;
            if (dto.Direccion != null) cliente.Direccion = dto.Direccion;
            if (dto.Comuna != null) cliente.Comuna = dto.Comuna;
            if (dto.Telefono != null) cliente.Telefono = dto.Telefono;
            if (dto.Correo != null) cliente.Correo = dto.Correo;
            if (dto.VisitasPorMes != null) cliente.VisitasPorMes = dto.VisitasPorMes.Value;
            if (dto.DiaPreferido != null) cliente.DiaPreferido = dto.DiaPreferido;
            if (dto.Observaciones != null) cliente.Observaciones = dto.Observaciones;
            if (dto.Activo != null) cliente.Activo = dto.Activo.Value;
            if (dto.NumeroClienteLegacy != null) cliente.NumeroClienteLegacy = dto.NumeroClienteLegacy;
            if (dto.TecnicoExternoId != null) cliente.TecnicoExternoId = dto.TecnicoExternoId.Value;

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
        [Authorize(Roles = "Administrador")]
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
