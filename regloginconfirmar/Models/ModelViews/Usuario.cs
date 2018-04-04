using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace regloginconfirmar.Models
{
    [MetadataType(typeof(UsuarioMetadata))]
    public partial class Usuario
    {
        public string ConfirmPassword { get; set; }
    }

    public class UsuarioMetadata
    {
        [Display(Name ="Nombre")]
        [Required(AllowEmptyStrings =false, ErrorMessage ="Nombre Requerido")]
        public string Nombre { get; set; }

        [Display(Name = "Apellido")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Apellido Requerido")]
        public string Apellido { get; set; }

        [Display(Name = "Correo Electronico")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Correo Requerido")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Display(Name = "Fecha de Nacimiento")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString ="{0:dd/MM/yyyy}")]
        public DateTime FechaNacimiento { get; set; }

        [Display(Name = "Contraseña")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Contraseña es Requerida")]
        [DataType(DataType.Password)]
        [MinLength(4,ErrorMessage ="Minimo 4 caracteres")]
        public string Password { get; set; }

        [Display(Name = "Confirmar Contraseña")]
        [DataType(DataType.Password)]
        [Compare("Password",ErrorMessage ="Contraseña no coincide")]
        public string ConfirmPassword { get; set; }


    }
}