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
        private readonly IEmailService _emailService;

        public VisitasController(ApplicationDbContext context, IConfiguration configuration, IImageService imageService, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _imageService = imageService;
            _emailService = emailService;
        }

        // GET: api/Visitas
        [HttpGet]
        public async Task<ActionResult<PaginatedResponseDto<VisitaResponseDto>>> GetVisitas(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            var query = _context.Visitas.AsNoTracking();

            var isAdmin = User.IsInRole("Administrador");
            if (!isAdmin)
            {
                var userTecnicoIdClaim = User.FindFirst("TecnicoId")?.Value;
                if (userTecnicoIdClaim == null)
                {
                    return StatusCode(403, new { mensaje = "Token inválido para técnico." });
                }
                int.TryParse(userTecnicoIdClaim, out int miTecnicoId);
                query = query.Where(v => v.TecnicoId == miTecnicoId);
            }

            if (fechaInicio.HasValue)
            {
                query = query.Where(v => v.FechaVisita >= fechaInicio.Value.Date);
            }
            if (fechaFin.HasValue)
            {
                // Incluir todo el día hasta las 23:59:59
                var finDia = fechaFin.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(v => v.FechaVisita <= finDia);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Ordenamos por fecha descendente y aplicamos paginación
            var visitas = await query
                .Include(v => v.Cliente)
                .Include(v => v.Tecnico)
                .OrderByDescending(v => v.FechaVisita)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
                    FotoUrl = v.FotoUrl,
                    FirmaClienteUrl = v.FirmaClienteUrl,
                    Cloro = v.Cloro,
                    Ph = v.Ph,
                    Retrolavado = v.Retrolavado,
                    Canastillos = v.Canastillos,
                    Aspirado = v.Aspirado,
                    Cepillado = v.Cepillado,
                    Llaves = v.Llaves,
                    Llenando = v.Llenando
                })
                .ToListAsync();

            var response = new PaginatedResponseDto<VisitaResponseDto>
            {
                Items = visitas,
                TotalCount = totalCount,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return Ok(response);
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
                    FotoUrl = v.FotoUrl,
                    FirmaClienteUrl = v.FirmaClienteUrl,
                    Cloro = v.Cloro,
                    Ph = v.Ph,
                    Retrolavado = v.Retrolavado,
                    Canastillos = v.Canastillos,
                    Aspirado = v.Aspirado,
                    Cepillado = v.Cepillado,
                    Llaves = v.Llaves,
                    Llenando = v.Llenando
                })
                .FirstOrDefaultAsync();

            if (visita == null)
            {
                return NotFound();
            }

            var isAdmin = User.IsInRole("Administrador");
            if (!isAdmin)
            {
                var userTecnicoIdClaim = User.FindFirst("TecnicoId")?.Value;
                if (userTecnicoIdClaim == null || visita.TecnicoId.ToString() != userTecnicoIdClaim)
                {
                    return StatusCode(403, new { mensaje = "No tienes permiso para ver esta visita." });
                }
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
                FotoUrl = dto.FotoUrl,
                FirmaClienteUrl = dto.FirmaClienteUrl,
                Cloro = dto.Cloro ?? false,
                Ph = dto.Ph ?? false,
                Retrolavado = dto.Retrolavado ?? false,
                Canastillos = dto.Canastillos ?? false,
                Aspirado = dto.Aspirado ?? false,
                Cepillado = dto.Cepillado ?? false,
                Llaves = dto.Llaves ?? false,
                Llenando = dto.Llenando ?? false
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

            var isAdmin = User.IsInRole("Administrador");
            if (!isAdmin)
            {
                var userTecnicoIdClaim = User.FindFirst("TecnicoId")?.Value;
                if (userTecnicoIdClaim == null || visita.TecnicoId.ToString() != userTecnicoIdClaim)
                {
                    return StatusCode(403, new { mensaje = "No tienes permiso para modificar una visita que no te pertenece." });
                }
            }

            var estadoAnterior = visita.Estado;

            // Bloquear Mass Assignment: Solo el administrador puede reasignar visitas o cambiar fechas
            if (isAdmin)
            {
                if (dto.ClienteId != null) visita.ClienteId = dto.ClienteId.Value;
                if (dto.TecnicoId != null) visita.TecnicoId = dto.TecnicoId.Value;
                if (dto.FechaVisita != null) visita.FechaVisita = dto.FechaVisita.Value;
            }
            if (dto.Estado != null) visita.Estado = (EstadoVisita)dto.Estado.Value;
            if (dto.Observaciones != null) visita.Observaciones = dto.Observaciones;
            if (!string.IsNullOrEmpty(dto.FotoUrl)) 
            {
                visita.FotoUrl = dto.FotoUrl;
            }
            if (!string.IsNullOrEmpty(dto.FirmaClienteUrl)) 
            {
                visita.FirmaClienteUrl = dto.FirmaClienteUrl;
            }

            if (dto.Cloro.HasValue) visita.Cloro = dto.Cloro.Value;
            if (dto.Ph.HasValue) visita.Ph = dto.Ph.Value;
            if (dto.Retrolavado.HasValue) visita.Retrolavado = dto.Retrolavado.Value;
            if (dto.Canastillos.HasValue) visita.Canastillos = dto.Canastillos.Value;
            if (dto.Aspirado.HasValue) visita.Aspirado = dto.Aspirado.Value;
            if (dto.Cepillado.HasValue) visita.Cepillado = dto.Cepillado.Value;
            if (dto.Llaves.HasValue) visita.Llaves = dto.Llaves.Value;
            if (dto.Llenando.HasValue) visita.Llenando = dto.Llenando.Value;

            try
            {
                await _context.SaveChangesAsync();

                // Si el estado cambió a Completada, enviar recibo por correo
                if (estadoAnterior != EstadoVisita.Completada && visita.Estado == EstadoVisita.Completada)
                {
                    // Cargar el cliente explícitamente para obtener su nombre y correo
                    await _context.Entry(visita).Reference(v => v.Cliente).LoadAsync();
                    
                    // Enviar correo de forma asíncrona sin bloquear la respuesta, o usando await normal
                    // Aquí usamos await normal para asegurar que capturemos errores si los hay. 
                    // Como el servicio está preparado con try-catch para la foto, no debería romper la app.
                    await _emailService.EnviarReciboAsync(visita);
                }
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
            var isAdmin = User.IsInRole("Administrador");
            if (!isAdmin)
            {
                var userTecnicoIdClaim = User.FindFirst("TecnicoId")?.Value;
                if (userTecnicoIdClaim == null || tecnicoId.ToString() != userTecnicoIdClaim)
                {
                    return StatusCode(403, new { mensaje = "No tienes permiso para ver la agenda de otro técnico." });
                }
            }

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
                    v.FirmaClienteUrl,
                    v.Cloro,
                    v.Ph,
                    v.Retrolavado,
                    v.Canastillos,
                    v.Aspirado,
                    v.Cepillado,
                    v.Llaves,
                    v.Llenando,
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
            var isAdmin = User.IsInRole("Administrador");
            if (!isAdmin)
            {
                var identityUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var tecnico = await _context.Tecnicos.FirstOrDefaultAsync(t => t.IdentityUserId == identityUserId);
                
                if (tecnico == null) return Forbid();

                if (tecnico.EsExterno)
                {
                    var cliente = await _context.Clientes.FindAsync(clienteId);
                    if (cliente == null || cliente.TecnicoExternoId != tecnico.Id)
                    {
                        return StatusCode(403, new { mensaje = "No tienes permiso para ver las visitas de este cliente." });
                    }
                }
                else
                {
                    return StatusCode(403, new { mensaje = "Técnicos internos no pueden ver el historial completo de un cliente." });
                }
            }

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

            var isAdmin = User.IsInRole("Administrador");
            if (!isAdmin)
            {
                var userTecnicoIdClaim = User.FindFirst("TecnicoId")?.Value;
                if (userTecnicoIdClaim == null || visita.TecnicoId.ToString() != userTecnicoIdClaim)
                {
                    return StatusCode(403, new { mensaje = "No tienes permiso para subir fotos a una visita que no te pertenece." });
                }
            }

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

        // POST: api/Visitas/5/firma
        // Permite subir la firma del cliente asociada a una visita. Se guarda en el contenedor "firmas".
        [HttpPost("{id}/firma")]
        public async Task<ActionResult<object>> SubirFirmaVisita(int id, IFormFile firma)
        {
            var visita = await _context.Visitas.FindAsync(id);
            if (visita == null)
                return NotFound("Visita no encontrada.");

            var isAdmin = User.IsInRole("Administrador");
            if (!isAdmin)
            {
                var userTecnicoIdClaim = User.FindFirst("TecnicoId")?.Value;
                if (userTecnicoIdClaim == null || visita.TecnicoId.ToString() != userTecnicoIdClaim)
                {
                    return StatusCode(403, new { mensaje = "No tienes permiso para subir una firma a una visita que no te pertenece." });
                }
            }

            try 
            {
                // Guardar en el contenedor "firmas" (y "firmas-dev" localmente por el ImageService)
                var firmaUrl = await _imageService.SubirImagenAsync(firma, "firmas");

                // Guardar en la BD
                visita.FirmaClienteUrl = firmaUrl;
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Firma subida con éxito", url = visita.FirmaClienteUrl });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
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