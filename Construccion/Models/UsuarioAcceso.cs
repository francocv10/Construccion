using System;
using System.ComponentModel.DataAnnotations;

namespace Construccion.Models
{
    public class UsuarioAcceso
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [Required]
        public DateTime FechaHoraAcceso { get; set; }

        [Required]
        [StringLength(45)]
        public string? DireccionIP { get; set; }

        public virtual Usuario? Usuario { get; set; }
    }
}
