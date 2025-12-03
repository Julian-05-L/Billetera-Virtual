using BilleteraVirtual.Models;
using BilleteraVirtual.Services;

namespace BilleteraVirtual.Services
{
    public interface ITokenService
    {
        string GenerateToken(Cliente cliente);
    }
}