// Modelo Bobeda
using System.ComponentModel.DataAnnotations;
namespace San_Agustin_Final.Models
{
    public class Bobeda
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "La división es obligatoria.")]
        [StringLength(50, ErrorMessage = "La división no puede exceder los 50 caracteres.")]
        public string Division { get; set; }
        [Required(ErrorMessage = "El estado es obligatorio.")]
        [RegularExpression("libre|arriendo|propietario", ErrorMessage = "El estado debe ser 'libre', 'arriendo' o 'propietario'.")]
        public string Estado { get; set; }
     
    }
}