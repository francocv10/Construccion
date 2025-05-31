using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Construccion.Models
{
    public partial class Usuario
    {
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "Debes ingresar en el campo el nombre completo del usuario")]
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto { get; set; } = null!;

        [Required(ErrorMessage = "Debes ingresa en el campo el identificador del usuario")]
        public string Identificador { get; set; } = null!;

        [Required(ErrorMessage = "Debes ingresa en el campo la clave para el usuario")]
        public string Clave { get; set; } = null!;

        [Display(Name = "Rol")]
        public int IdRol { get; set; }
        [Display(Name = "Rol")]
        public virtual Rol? IdRolNavigation { get; set; }

    }
}