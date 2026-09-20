using System.ComponentModel.DataAnnotations;

namespace PiscinerosAPI.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        

        public string Nombre { get; set; } = string.Empty;

        public string Direccion { get; set; } = string.Empty;

        public string Comuna { get; set; } = string.Empty;

        public string Telefono { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        [Required]
        public int VisitasPorMes { get; set; }
        
        public string DiaPreferido { get; set; } = string.Empty;

        public string Observaciones { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;
    }
}
