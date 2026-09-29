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
    public class TecnicosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser> _userManager;

        public TecnicosController(ApplicationDbContext context, Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/tecnicos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TecnicoDto>>> GetTecnicos()
        {
            return await _context.Tecnicos
                .AsNoTracking()
                .Select(t => new TecnicoDto
                {
                    Id = t.Id,
                    Nombre = t.Nombre,
                    RUT = t.RUT,
                    Telefono = t.Telefono,
                    MontoPorVisita = t.MontoPorVisita,
                    Correo = t.Correo,
                    Activo = t.Activo,
                    EsExterno = t.EsExterno
                })
                .ToListAsync();
        }

        // GET: api/tecnicos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TecnicoDto>> GetTecnico(int id)
        {
            var tecnico = await _context.Tecnicos
                .AsNoTracking()
                .Where(t => t.Id == id)
                .Select(t => new TecnicoDto
                {
                    Id = t.Id,
                    Nombre = t.Nombre,
                    RUT = t.RUT,
                    Telefono = t.Telefono,
                    MontoPorVisita = t.MontoPorVisita,
                    Correo = t.Correo,
                    Activo = t.Activo,
                    EsExterno = t.EsExterno
                })
                .FirstOrDefaultAsync();

            if (tecnico == null)
            {
                return NotFound();
            }

            return tecnico;
        }

        // POST: api/tecnicos
        [HttpPost]
        public async Task<ActionResult<TecnicoDto>> PostTecnico(TecnicoRequestDto dto)
        {
            if (string.IsNullOrEmpty(dto.Correo) || string.IsNullOrEmpty(dto.Password))
            {
                return BadRequest("El correo y la contraseña son obligatorios para crear la cuenta del técnico.");
            }

            // 1. Create IdentityUser
            var user = new Microsoft.AspNetCore.Identity.IdentityUser
            {
                UserName = dto.Correo,
                Email = dto.Correo
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // 2. Assign Role
            await _userManager.AddToRoleAsync(user, "Tecnico");

            // 3. Create Tecnico record linked to the IdentityUser
            var tecnico = new Tecnico
            {
                Nombre = dto.Nombre ?? "",
                RUT = dto.RUT ?? "",
                Telefono = dto.Telefono ?? "",
                MontoPorVisita = dto.MontoPorVisita ?? 0,
                Correo = dto.Correo,
                IdentityUserId = user.Id,
                Activo = dto.Activo ?? true,
                EsExterno = dto.EsExterno ?? false
            };

            _context.Tecnicos.Add(tecnico);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTecnico), new { id = tecnico.Id }, new { id = tecnico.Id });
        }

        // PUT: api/tecnicos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTecnico(int id, TecnicoRequestDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest();
            }

            var tecnico = await _context.Tecnicos.FindAsync(id);
            if (tecnico == null) return NotFound();

            if (dto.Nombre != null) tecnico.Nombre = dto.Nombre;
            if (dto.RUT != null) tecnico.RUT = dto.RUT;
            if (dto.Telefono != null) tecnico.Telefono = dto.Telefono;
            if (dto.MontoPorVisita != null) tecnico.MontoPorVisita = dto.MontoPorVisita.Value;
            if (dto.Correo != null) tecnico.Correo = dto.Correo;
            if (dto.Activo != null) tecnico.Activo = dto.Activo.Value;
            if (dto.EsExterno != null) tecnico.EsExterno = dto.EsExterno.Value;

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

        // DELETE: api/tecnicos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTecnico(int id)
        {
            var tecnico = await _context.Tecnicos.FindAsync(id);
            if (tecnico == null)
            {
                return NotFound();
            }

            _context.Tecnicos.Remove(tecnico);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TecnicoExists(int id)
        {
            return _context.Tecnicos.Any(e => e.Id == id);
        }
    }
}