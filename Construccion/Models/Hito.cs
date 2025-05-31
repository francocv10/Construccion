using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Construccion.Models
{
    public class Hito
    {
        [Key]
        public int IdHito { get; set; }

        public int IdObra { get; set; }
        public Obra? Obra { get; set; }

        public int? IdPartida { get; set; }  
        public Partida? Partida { get; set; }  

        [Required(ErrorMessage = "Debes ingresar el nombre del hito")]
        [StringLength(100, ErrorMessage = "El nombre del hito no puede exceder los 100 caracteres")]
        public string? NombreHito { get; set; }

        public bool Finalizado { get; set; } = false;
    }

}