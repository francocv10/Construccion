using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Construccion.Models
{
    public class SeguimientoObra
    {
        [Key]
        public int IdSeguimiento { get; set; }

        [Required]
        public int IdObra { get; set; }
        public Obra? Obra { get; set; }

        public DateTime FechaSeguimiento { get; set; } = DateTime.Now;
    }
}

