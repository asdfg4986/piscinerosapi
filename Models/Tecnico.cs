using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PiscinerosAPI.Models
{
    public class Tecnico
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        
        public string RUT { get; set; } = string.Empty;

        public string Telefono { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoPorVisita { get; set; }

        public string Correo { get; set; } = string.Empty;

        // The bridge to AspNetUsers table
        public string? IdentityUserId { get; set; }
    }
}
