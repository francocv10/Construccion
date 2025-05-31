using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Construccion.Models
{
    public class SalidaMaterial
    {
        [Key]
        public int IdSalida { get; set; }

        [ForeignKey("Bodega")]
        public int IdBodega { get; set; }
        public Bodega? Bodega { get; set; }

        [ForeignKey("Insumo")]
        public int IdInsumo { get; set; }
        public Insumos? Insumo { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero")]
        public int Cantidad { get; set; }

        public DateTime FechaSalida { get; set; } = DateTime.Now;

        public int? IdObra { get; set; }
        public Obra? Obra { get; set; }

    }
}

