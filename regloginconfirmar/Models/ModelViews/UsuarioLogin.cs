using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace regloginconfirmar.Models.ModelViews
{
    public class UsuarioLogin
    {
        [Display(Name ="Correo Electronico")]
        [Required(ErrorMessage ="Correo Electronico es Requerido")]
        public string Email { get; set; }

        [Required(ErrorMessage ="Contraseña es Requerida")]
        [DataType(DataType.Password)]
        public string  Password { get; set; }

        [Display(Name ="Recordar")]
        public bool RememberMe { get; set; }
    }
}