using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace regloginconfirmar.Models.ModelViews
{
    public class UsuarioResetPass
    {
        [Required(ErrorMessage ="Ingrese su nueva Contraseña", AllowEmptyStrings = false)]
        [DataType(DataType.Password)]
        public string NuevaPassword { get; set; }

        [Required(ErrorMessage = "Debe confirmar su nueva Contraseña")]
        [DataType(DataType.Password)]
        public string ConfirmarPassword { get; set; }

        [Required]
        public string Cod_Reset { get; set; }
    }
}