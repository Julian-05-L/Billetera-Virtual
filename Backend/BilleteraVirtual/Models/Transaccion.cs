using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BilleteraVirtual.Models
{
    public class Transaccion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El código de la criptomoneda es obligatorio")]
        public string? CryptoCode { get; set; }

        [Required(ErrorMessage = "La acción es obligatoria")]
        public string? Action { get; set; } // "purchase" o "sale"

        [Required]
        [Range(0.000000001, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero")]
        public decimal CryptoAmount { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El dinero debe ser mayor que cero")]
        public decimal Money { get; set; }

        public DateTime FechaTransaccion { get; set; }

        [Required(ErrorMessage = "El ID del cliente es obligatorio")]
        public int ClienteId { get; set; }

        [JsonIgnore]
        public Cliente? Cliente { get; set; } // Relación con Cliente
    }
}
