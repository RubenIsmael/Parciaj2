using System.ComponentModel.DataAnnotations;

namespace San_Agustin_Final.Models
{
    public class HistorialPago
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El ID de la reserva es obligatorio.")]
        public int IdReserva { get; set; }

        [Required(ErrorMessage = "El monto del pago es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero.")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "La fecha del pago es obligatoria.")]
        public DateTime FechaPago { get; set; }

        // Navegación
        public virtual Reserva Reserva { get; set; }
    }
}