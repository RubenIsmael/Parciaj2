using System.ComponentModel.DataAnnotations;

namespace San_Agustin_Final.Models
{
    public class Reserva
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La cédula es obligatoria.")]
        public int IdCliente { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(100, ErrorMessage = "El apellido no puede exceder los 100 caracteres.")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El ID del precio es obligatorio.")]
        public int IdPrecio { get; set; }

        [Required(ErrorMessage = "El estado de pago es obligatorio.")]
        [RegularExpression("pagado|pendiente", ErrorMessage = "El estado de pago debe ser 'pagado' o 'pendiente'.")]
        public string EstadoPago { get; set; }

        [Required(ErrorMessage = "La fecha de reserva es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime FechaReserva { get; set; }

        // Relaciones
        public virtual Precio Precio { get; set; }
        public virtual Cliente Cliente { get; set; }
    }
}
