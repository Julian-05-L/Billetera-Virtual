using System.ComponentModel.DataAnnotations;

namespace BilleteraVirtual.DTOs
{
    public class CrearTransaccionDTO    
    {
        [Required(ErrorMessage = "El codigo de la criptomoneda es obligatorio")]
        public string? CryptoCode { get; set; }

        [Required(ErrorMessage = "La accion es obligatoria")]
        [RegularExpression("purchase|sale", ErrorMessage = "Debe ser 'purchase' o 'sale'")]
        public string? Action { get; set; }

        [Required]
        [Range(0.000000001, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero")]
        public decimal CryptoAmount { get; set; }

        [Required(ErrorMessage = "El ID del cliente es obligatorio")]
        public int ClienteId { get; set; }
    }
}
