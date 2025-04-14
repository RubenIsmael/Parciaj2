using System.ComponentModel.DataAnnotations;

namespace San_Agustin_Final.Models
{
    public class Contrato
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El ID de la reserva es obligatorio.")]
        public int IdReserva { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de finalización es obligatoria.")]
        public DateTime FechaFin { get; set; }

        [StringLength(500, ErrorMessage = "Los términos del contrato no pueden exceder los 500 caracteres.")]
        public string Terminos { get; set; }

        // Navegación
        public virtual Reserva Reserva { get; set; }
    }
}