using System.ComponentModel.DataAnnotations;

namespace BilleteraVirtual.DTOs
{
    public class CrearClienteDTO
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe contener al menos 3 letras")]
        public string? Nombre { get; set; }

        [Required(ErrorMessage = "El correo electronico es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo electronico no es valido")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "La contraseña debe contener al menos 6 caracteres")]
        public string? Password { get; set; }
    }
}
