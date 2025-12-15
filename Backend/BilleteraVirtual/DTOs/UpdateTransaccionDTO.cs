using System.ComponentModel.DataAnnotations;

namespace BilleteraVirtual.DTOs
{
    public class UpdateTransaccionDTO
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string? CryptoCode { get; set; }
        [Required]
        public string? Action { get; set; }
        [Required]
        public decimal CryptoAmount { get; set; }
        [Required]
        public DateTime FechaTransaccion { get; set; }
    }
}