using System.ComponentModel.DataAnnotations;

namespace Construccion.Models
{
    public class Bodega
    {
        [Key]
        public int IdBodega { get; set; }

        [Display(Name = "Nombre Bodega")]
        [Required (ErrorMessage ="Debes ingresar el nombre de bodega")]
        [StringLength(17, ErrorMessage = "EL campo solamente permite 17 carácteres")]
        public string NombreBodega { get; set; } = null!;

        public List<Insumos>? Insumos { get; set; }
    }
}
