using System.ComponentModel.DataAnnotations;

namespace Construccion.Models
{
    public class Insumos
    {
        [Key]
        public int IdInsumos { get; set; }

        [Display(Name = "Nombre Insumo")]
        [Required(ErrorMessage ="Debes ingresar el nombre del material o herramienta")]
        public string? Nombre { get; set; } = null!;

        [Required(ErrorMessage ="Debes ingresar la cantidad del material o herramienta")]
        [RegularExpression(@"^\d+$", ErrorMessage ="Solamente se permiten números")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage ="Debes Ingresar el tipo de material o el tipo de herramienta")]
        [StringLength(24,ErrorMessage ="Solamente se permiten ingresar 24 carácteres")]
        public string? Tipo { get; set; } = null!;
        public int IdBodega { get; set; }
        public Bodega? Bodega { get; set; }

    }
}
