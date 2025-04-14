// Modelo TipoBobeda
using System.ComponentModel.DataAnnotations;
namespace San_Agustin_Final.Models
{
    public class TipoBobeda
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "El nombre del tipo de bóveda es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
        public string Nombre { get; set; }
        [StringLength(100, ErrorMessage = "La descripción no puede exceder los 100 caracteres.")]
        public string Descripcion { get; set; }
    }
}