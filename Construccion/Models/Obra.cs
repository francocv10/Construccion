using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Construccion.Models
{
    [Table("Obras")]
    public class Obra
    {
        [Key]
        public int IdObra { get; set; }

        [Required(ErrorMessage = "Debes ingresar el nombre de la obra")]
        public string? NombreObra { get; set; }  

        [Required(ErrorMessage = "Debes ingresar el nombre del cliente")]
        public string? Cliente { get; set; }

        public List<Partida> Partidas { get; set; } = new List<Partida>();


        public decimal TotalPresupuesto
        {
            get
            {
                return Partidas.Sum(p => p.Subtotal); 
            }
        }

        public decimal IVA
        {
            get
            {
                return TotalPresupuesto * 0.19m;
            }
        }

        public decimal Total
        {
            get
            {
                return TotalPresupuesto + IVA;
            }
        }
    }
}
