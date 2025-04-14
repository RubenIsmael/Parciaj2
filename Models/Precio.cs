using System.ComponentModel.DataAnnotations;

namespace San_Agustin_Final.Models
{
    public class Precio
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El sector es obligatorio.")]
        [StringLength(50, ErrorMessage = "El sector no puede exceder los 50 caracteres.")]
        public string Sector { get; set; }
  
        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que cero.")]
        public decimal PrecioValor { get; set; }
    }
}