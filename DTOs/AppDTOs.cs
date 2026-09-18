namespace PiscinerosAPI.DTOs
{
    public class ClienteDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Comuna { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public int VisitasPorMes { get; set; }
        public string DiaPreferido { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class ClienteRequestDto
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Direccion { get; set; }
        public string? Comuna { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public int? VisitasPorMes { get; set; }
        public string? DiaPreferido { get; set; }
        public string? Observaciones { get; set; }
        public bool? Activo { get; set; }
    }

    public class TecnicoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string RUT { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public decimal MontoPorVisita { get; set; }
        public string Correo { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class TecnicoRequestDto
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? RUT { get; set; }
        public string? Telefono { get; set; }
        public decimal? MontoPorVisita { get; set; }
        public string? Correo { get; set; }
        public string? Password { get; set; }
        public bool? Activo { get; set; }
    }

    // DTO para cuando se consulta una Visita (GET)
    public class VisitaResponseDto
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public ClienteDto? Cliente { get; set; }

        public int TecnicoId { get; set; }
        public TecnicoDto? Tecnico { get; set; }

        public DateTime FechaVisita { get; set; }
        public int Estado { get; set; }
        public string? Observaciones { get; set; }
        public string? FotoUrl { get; set; }
    }

    // DTO para cuando se crea/actualiza una Visita (POST/PUT)
    public class VisitaRequestDto
    {
        public int Id { get; set; }
        public int? ClienteId { get; set; }
        public int? TecnicoId { get; set; }
        public DateTime? FechaVisita { get; set; }
        public int? Estado { get; set; }
        public string? Observaciones { get; set; }
        public string? FotoUrl { get; set; }
    }
}
