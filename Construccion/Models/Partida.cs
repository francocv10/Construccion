using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq; 

namespace Construccion.Models
{
    public class Partida
    {
        [Key]
        public int IdPartida { get; set; }

        [Required(ErrorMessage = "Debes ingresar el nombre de la partida")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "Debes ingresar el material a la partida")]
        public List<Material> Materiales { get; set; } = new List<Material>();

        [Required(ErrorMessage = "Debes ingresar el costo de mano de obra")]
        [RegularExpression(@"^\d+(\. \d{1,2})?$", ErrorMessage ="El costo de la mano debe Obra de ser un número válido")]
        public decimal ManoDeObra { get; set; }

        public decimal TotalMateriales
        {
            get
            {
                return Materiales.Sum(m => m.Cantidad * m.Precio);
            }
        }

        public decimal Subtotal
        {
            get
            {
                return TotalMateriales + ManoDeObra;
            }
        }

        public int IdObra { get; set; }
        public Obra? Obra { get; set; }
    }
}


