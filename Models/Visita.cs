using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PiscinerosAPI.Models
{
    // Definimos los múltiples estados posibles
    public enum EstadoVisita
    {
        Programada = 0,     // Estado inicial al crearla
        EnCamino = 1,       // El técnico va en ruta
        Completada = 2,     // Trabajo terminado con éxito
        Cancelada = 3,      // El cliente canceló
        Fallida = 4         // El técnico llegó pero no pudo entrar
    }

    public class Visita
    {
        public int Id { get; set; }

        // Relación con el Cliente
        [Required]
        public int ClienteId { get; set; }
        [ForeignKey("ClienteId")]
        public Cliente? Cliente { get; set; }

        // Relación con el Técnico
        [Required]
        public int TecnicoId { get; set; }
        [ForeignKey("TecnicoId")]
        public Tecnico? Tecnico { get; set; }

        [Required]
        public DateTime FechaVisita { get; set; }

        public EstadoVisita Estado { get; set; } = EstadoVisita.Programada;

        public string? Observaciones { get; set; }

        public string? FotoUrl { get; set; } // URL de la foto tomada durante la visita
        public string? FirmaClienteUrl { get; set; } // URL de la firma opcional del cliente

        // Tareas del recibo
        public bool Cloro { get; set; }
        public bool Ph { get; set; }
        public bool Retrolavado { get; set; }
        public bool Canastillos { get; set; }
        public bool Aspirado { get; set; }
        public bool Cepillado { get; set; }
        public bool Llaves { get; set; }
        public bool Llenando { get; set; }
    }
}