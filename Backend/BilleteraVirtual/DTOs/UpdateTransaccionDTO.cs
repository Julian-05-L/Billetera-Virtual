using System.ComponentModel.DataAnnotations;

namespace BilleteraVirtual.DTOs
{
    public class UpdateTransaccionDTO
    {
        public int Id { get; set; }
        public string? CryptoCode { get; set; }
        public string? Action { get; set; }
        public decimal CryptoAmount { get; set; }
        public DateTime FechaTransaccion { get; set; }
    }
}