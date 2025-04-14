using System.ComponentModel.DataAnnotations;
using San_Agustin_Final.Validations;

namespace San_Agustin_Final.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        [StringLength(50, ErrorMessage = "Los nombres no pueden exceder los 50 caracteres.")]
        public string Nombres { get; set; }

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        [StringLength(50, ErrorMessage = "Los apellidos no pueden exceder los 50 caracteres.")]
        public string Apellidos { get; set; }

        [Required(ErrorMessage = "La cédula es obligatoria.")]
        [StringLength(10, ErrorMessage = "La cédula debe tener 10 caracteres.")]
        [CedulaEcuatoriana(ErrorMessage = "La cédula ingresada no es válida.")]
        public string Clave { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no es válido.")]
        [StringLength(30, ErrorMessage = "El correo no puede exceder los 30 caracteres.")]
        public string Correo { get; set; }
    }
}