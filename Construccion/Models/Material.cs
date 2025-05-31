using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Construccion.Models
{
    public class Material
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Debes ingresar el material")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "Debes ingresar la cantidad del material")]
        [RegularExpression(@"^\d+(\. \d{1,2})?$", ErrorMessage = "La cantidad debe ser un número válido")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "Debes ingresar el precio del material")]
        [RegularExpression(@"^\d+(\. \d{1,2})?$", ErrorMessage = "El precio debe ser un número válido")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "Debes ingresar el tipo de unidad")]
        public string TipoUnidad { get; set; } = null!;

        public int? IdPartida { get; set; }
        public Partida? Partida { get; set; }
    }
}
