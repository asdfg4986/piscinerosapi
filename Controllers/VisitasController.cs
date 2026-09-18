using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PiscinerosAPI.Models;
using Microsoft.AspNetCore.Authorization;
using PiscinerosAPI.Services;
using PiscinerosAPI.DTOs;

namespace PiscinerosAPI.Controllers
{
    [Authorize(Roles = "Administrador, Tecnico")]
    [Route("api/[controller]")]
    [ApiController]
    public class VisitasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IImageService _imageService;

        public VisitasController(ApplicationDbContext context, IConfiguration configuration, IImageService imageService)
        {
            _context = context;
            _configuration = configuration;
            _imageService = imageService;
        }

        // GET: api/Visitas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VisitaResponseDto>>> GetVisitas()
        {
            var visitas = await _context.Visitas
                .AsNoTracking()
                .Include(v => v.Cliente)
                .Include(v => v.Tecnico)
                .Select(v => new VisitaResponseDto
                {
                    Id = v.Id,
                    ClienteId = v.ClienteId,
                    Cliente = v.Cliente == null ? null : new ClienteDto
                    {
                        Id = v.Cliente.Id,
                        Nombre = v.Cliente.Nombre,
                        Direccion = v.Cliente.Direccion,
                        Comuna = v.Cliente.Comuna,
                        Telefono = v.Cliente.Telefono,
                        Correo = v.Cliente.Correo,
                        VisitasPorMes = v.Cliente.VisitasPorMes,
                        DiaPreferido = v.Cliente.DiaPreferido ?? "",
                        Observaciones = v.Cliente.Observaciones ?? ""
                    },
                    TecnicoId = v.TecnicoId,
                    Tecnico = v.Tecnico == null ? null : new TecnicoDto
                    {
                        Id = v.Tecnico.Id,
                        Nombre = v.Tecnico.Nombre,
                        RUT = v.Tecnico.RUT ?? "",
                        Telefono = v.Tecnico.Telefono ?? "",
                        MontoPorVisita = v.Tecnico.MontoPorVisita
                    },
                    FechaVisita = v.FechaVisita,
                    Estado = (int)v.Estado,
                    Observaciones = v.Observaciones,
                    FotoUrl = v.FotoUrl
                })
                .ToListAsync();

            return Ok(visitas);
        }

        // GET: api/Visitas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VisitaResponseDto>> GetVisita(int id)
        {
            var visita = await _context.Visitas
                .AsNoTracking()
                .Include(v => v.Cliente)
                .Include(v => v.Tecnico)
                .Where(v => v.Id == id)
                .Select(v => new VisitaResponseDto
                {
                    Id = v.Id,
                    ClienteId = v.ClienteId,
                    Cliente = v.Cliente == null ? null : new ClienteDto
                    {
                        Id = v.Cliente.Id,
                        Nombre = v.Cliente.Nombre,
                        Direccion = v.Cliente.Direccion,
                        Comuna = v.Cliente.Comuna,
                        Telefono = v.Cliente.Telefono,
                        Correo = v.Cliente.Correo,
                        VisitasPorMes = v.Cliente.VisitasPorMes,
                        DiaPreferido = v.Cliente.DiaPreferido ?? "",
                        Observaciones = v.Cliente.Observaciones ?? ""
                    },
                    TecnicoId = v.TecnicoId,
                    Tecnico = v.Tecnico == null ? null : new TecnicoDto
                    {
                        Id = v.Tecnico.Id,
                        Nombre = v.Tecnico.Nombre,
                        RUT = v.Tecnico.RUT ?? "",
                        Telefono = v.Tecnico.Telefono ?? "",
                        MontoPorVisita = v.Tecnico.MontoPorVisita
                    },
                    FechaVisita = v.FechaVisita,
                    Estado = (int)v.Estado,
                    Observaciones = v.Observaciones,
                    FotoUrl = v.FotoUrl
                })
                .FirstOrDefaultAsync();

            if (visita == null)
            {
                return NotFound();
            }

            return Ok(visita);
        }


        // POST: api/Visitas
        [Authorize(Roles = "Administrador, Tecnico")]
        [HttpPost]
        public async Task<ActionResult<VisitaResponseDto>> PostVisita(VisitaRequestDto dto)
        {
            var visita = new Visita
            {
                ClienteId = dto.ClienteId ?? 0,
                TecnicoId = dto.TecnicoId ?? 0,
                FechaVisita = dto.FechaVisita ?? DateTime.Now,
                Estado = dto.Estado.HasValue ? (EstadoVisita)dto.Estado.Value : EstadoVisita.Programada,
                Observaciones = dto.Observaciones,
                FotoUrl = dto.FotoUrl
            };

            _context.Visitas.Add(visita);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVisita), new { id = visita.Id }, new { id = visita.Id });
        }

        // PUT: api/Visitas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVisita(int id, VisitaRequestDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("El ID de la ruta no coincide con el ID del cuerpo (JSON).");
            }

            var visita = await _context.Visitas.FindAsync(id);
            if (visita == null) return NotFound();

            if (dto.ClienteId != null) visita.ClienteId = dto.ClienteId.Value;
            if (dto.TecnicoId != null) visita.TecnicoId = dto.TecnicoId.Value;
            if (dto.FechaVisita != null) visita.FechaVisita = dto.FechaVisita.Value;
            if (dto.Estado != null) visita.Estado = (EstadoVisita)dto.Estado.Value;
            if (dto.Observaciones != null) visita.Observaciones = dto.Observaciones;
            if (!string.IsNullOrEmpty(dto.FotoUrl)) 
            {
                visita.FotoUrl = dto.FotoUrl;
            }

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

        // GET: api/Visitas/tecnico/5/fecha/2026-08-13
        // Endpoint para obtener visitas por técnico y fecha
        [HttpGet("tecnico/{tecnicoId}/fecha/{fecha}")]
        public async Task<ActionResult<IEnumerable<object>>> GetVisitasPorFecha(int tecnicoId, DateTime fecha)
        {
            // 1. Establecemos los límites del día (desde las 00:00:00 hasta justo antes del día siguiente)
            var inicioDelDia = fecha.Date;
            var inicioDelDiaSiguiente = inicioDelDia.AddDays(1);

            // 2. Buscamos en la base de datos
            var visitas = await _context.Visitas
                .AsNoTracking()
                .Include(v => v.Cliente) // Traemos los datos del cliente para que el técnico sepa a dónde ir
                .Include(v => v.Tecnico)
                .Where(v => v.TecnicoId == tecnicoId &&
                            v.FechaVisita >= inicioDelDia &&
                            v.FechaVisita < inicioDelDiaSiguiente)
                .OrderBy(v => v.FechaVisita) // Ordenamos cronológicamente para que la ruta tenga sentido
                .Select(v => new
                {
                    v.Id,
                    v.FechaVisita,
                    v.Estado,
                    v.Observaciones,
                    v.FotoUrl,
                    Cliente = new
                    {
                        v.Cliente.Nombre,
                        v.Cliente.Direccion,
                        v.Cliente.Comuna
                    },
                    Tecnico = new
                    {
                        v.Tecnico.Nombre
                    }
                })
                .ToListAsync();

            return Ok(visitas);
        }

        // GET: api/Visitas/cliente/5
        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<Visita>>> GetVisitasPorCliente(int clienteId)
        {
            // Buscamos en la tabla Visitas todas las que coincidan con el ClienteId
            var visitas = await _context.Visitas
                                        .AsNoTracking()
                                        .Include (v => v.Tecnico)
                                        .Where(v => v.ClienteId == clienteId)
                                        .OrderByDescending(v => v.FechaVisita) // Ordenamos para que la más reciente salga primero
                                        .ToListAsync();

            // Aunque no tenga visitas, devolvemos un 200 OK con una lista vacía.
            return Ok(visitas);
        }

        // POST: api/Visitas/5/foto
        // Permite subir una foto asociada a una visita específica. La foto se almacena en Azure Blob Storage y se guarda la URL en la base de datos.
        [HttpPost("{id}/foto")]
        public async Task<ActionResult<object>> SubirFotoVisita(int id, IFormFile foto)
        {
            var visita = await _context.Visitas.FindAsync(id);
            if (visita == null)
                return NotFound("Visita no encontrada.");

            try 
            {
                // Usar el ImageService para manejar la lógica de guardado
                var fotoUrl = await _imageService.SubirImagenAsync(foto, "fotos");

                // Guardar en la BD
                visita.FotoUrl = fotoUrl;
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Foto subida con éxito", url = visita.FotoUrl });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Devolvemos el error real para poder diagnosticarlo en el frontend
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
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