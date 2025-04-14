using System;
using System.ComponentModel.DataAnnotations;

namespace San_Agustin_Final.Models
{
    public class Mensajes
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre completo no puede exceder los 100 caracteres.")]
        public string NombreCompleto { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        public string CorreoElectronico { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [Phone(ErrorMessage = "El formato del teléfono no es válido.")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "El mensaje es obligatorio.")]
        [StringLength(1000, ErrorMessage = "El mensaje no puede exceder los 1000 caracteres.")]
        public string Mensaje { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Nueva propiedad para indicar si el mensaje ha sido leído
        public bool Leido { get; set; } = false; // Por defecto, no leído
    }
}