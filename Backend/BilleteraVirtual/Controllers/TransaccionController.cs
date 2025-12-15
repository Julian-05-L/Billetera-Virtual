using Microsoft.AspNetCore.Mvc;
using BilleteraVirtual.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Writers;
using System.Security.Cryptography;
using System.Text.Json;
using System.Runtime.InteropServices.Marshalling;
using System.Diagnostics;

namespace BilleteraVirtual.Controllers
{
    [Route("transaccion")]
    [ApiController]
    public class TransaccionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        public TransaccionController(AppDbContext context)
        {
            _context = context;
            _httpClient = new HttpClient();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transaccion>>> ObtenerTransaccion()
        {
            return await _context.Transacciones.Include(t => t.Cliente).ToListAsync();
        }

        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<Transaccion>> ObtenerTransaccionPorId(int clienteId)
        {
            var transacciones = await _context.Transacciones
                .Where(t => t.ClienteId == clienteId)
                .ToListAsync();
            if (transacciones == null || !transacciones.Any())
            {
                return NotFound("Este cliente no tiene transacciones");
            }
            return Ok(transacciones);
        }

        [HttpPost]
        public async Task<ActionResult> CrearTransaccion(BilleteraVirtual.DTOs.CrearTransaccionDTO crearTransaccionDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var cliente = await _context.Clientes.FindAsync(crearTransaccionDTO.ClienteId);
            if (cliente == null)
            {
                return BadRequest("Cliente no encontrado.");
            }

            decimal precioARS;
            try
            {
                var url = $"https://criptoya.com/api/argenbtc/{crearTransaccionDTO.CryptoCode}/ars";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(json);
                precioARS = jsonDoc.RootElement.GetProperty("totalAsk").GetDecimal();

            }
            catch 
            {
                return BadRequest("Error al obtener el precio de la criptomoneda.");
            }

            if (crearTransaccionDTO.Action.ToLower() == "sale")
            {
                var totalComprado = _context.Transacciones
                    .Where(t => t.ClienteId == crearTransaccionDTO.ClienteId && t.Action.ToLower() == "purchase" && t.CryptoCode == crearTransaccionDTO.CryptoCode)
                    .Sum(t => t.CryptoAmount);

                var totalVendido = _context.Transacciones
                    .Where(t => t.ClienteId == crearTransaccionDTO.ClienteId && t.Action.ToLower() == "sale" && t.CryptoCode == crearTransaccionDTO.CryptoCode)
                    .Sum(t => t.CryptoAmount);

                var saldoDisponible = totalComprado - totalVendido;

                if (saldoDisponible < crearTransaccionDTO.CryptoAmount)
                {
                    return BadRequest("Saldo insuficiente para realizar la venta.");
                }
                var totalVendidoEnPesos = precioARS * crearTransaccionDTO.CryptoAmount;
                cliente.SaldoPesos += totalVendidoEnPesos;
                _context.Clientes.Update(cliente);
            }
            else if (crearTransaccionDTO.Action.ToLower() == "purchase")
            {
                var totalComprado = precioARS * crearTransaccionDTO.CryptoAmount;

                if (totalComprado > cliente.SaldoPesos)  
                {
                    return BadRequest("Saldo insuficiente para realizar la compra.");
                }
                cliente.SaldoPesos -= totalComprado;
                _context.Clientes.Update(cliente);
            }
                var transaccion = new Transaccion
                {
                    CryptoCode = crearTransaccionDTO.CryptoCode,
                    Action = crearTransaccionDTO.Action,
                    CryptoAmount = crearTransaccionDTO.CryptoAmount,
                    ClienteId = crearTransaccionDTO.ClienteId,
                    Money = precioARS * crearTransaccionDTO.CryptoAmount,
                    FechaTransaccion = DateTime.UtcNow
                };

            _context.Transacciones.Add(transaccion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(ObtenerTransaccion), new { id = transaccion.Id }, transaccion);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateTransaccion(int id, BilleteraVirtual.DTOs.UpdateTransaccionDTO updateTransaccionDTO)
        {
            if (id != updateTransaccionDTO.Id)
            {
                return BadRequest("El Id de la transacción no coincide");
            }

            var transaccionExistente = await _context.Transacciones.FindAsync(id);

            if (transaccionExistente == null) 
            {
                return NotFound("Transacción no encontrada");
            }

            var cliente = await _context.Clientes.FindAsync(transaccionExistente.ClienteId);

            if (cliente == null) 
            { 
                return BadRequest("Cliente no válido");
            }

            // reversar el efecto de la transacción existente en el saldo del cliente
            if (transaccionExistente.Action.ToLower() == "purchase")
            {
                cliente.SaldoPesos += transaccionExistente.Money;
            }
            else if (transaccionExistente.Action.ToLower() == "sale") 
            { 
                cliente.SaldoPesos -= transaccionExistente.Money;
            }

            // obtener el precio actual 
            decimal precioARS;
            try
            {
                var url = $"https://criptoya.com/api/argenbtc/{updateTransaccionDTO.CryptoCode}/ars";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(json);
                precioARS = jsonDoc.RootElement.GetProperty("totalAsk").GetDecimal();
            }
            catch
            {
                return BadRequest("Error al obtener el precio de la criptomoneda.");
            }

            decimal nuevoMontoPesos = updateTransaccionDTO.CryptoAmount * precioARS;

            // validacion de acciones
            if (updateTransaccionDTO.Action.ToLower() == "purchase")
            {
                if (nuevoMontoPesos > cliente.SaldoPesos) 
                { 
                    return BadRequest("Saldo insuficiente para realizar esta compra modificada.");
                }

                cliente.SaldoPesos -= nuevoMontoPesos;
            }
            else if (updateTransaccionDTO.Action.ToLower() == "sale")
            {
                var totalComprado = _context.Transacciones
                    .Where(t => t.ClienteId == cliente.Id &&
                                t.Action.ToLower() == "purchase" &&
                                t.CryptoCode == updateTransaccionDTO.CryptoCode)
                    .Sum(t => t.CryptoAmount);

                var totalVendido = _context.Transacciones
                    .Where(t => t.ClienteId == cliente.Id &&
                                t.Action.ToLower() == "sale" &&
                                t.CryptoCode == updateTransaccionDTO.CryptoCode &&
                                t.Id != transaccionExistente.Id)
                    .Sum(t => t.CryptoAmount);

                var saldoDisponible = totalComprado - totalVendido;

                if (saldoDisponible < updateTransaccionDTO.CryptoAmount) 
                {
                    return BadRequest("Saldo insuficiente de esa criptomoneda para esta venta.");
                }

                cliente.SaldoPesos += nuevoMontoPesos;
            }

            // actualizar cliente
            _context.Clientes.Update(cliente);

            // actualizar transaccion
            transaccionExistente.CryptoCode = updateTransaccionDTO.CryptoCode;
            transaccionExistente.Action = updateTransaccionDTO.Action;
            transaccionExistente.CryptoAmount = updateTransaccionDTO.CryptoAmount;
            transaccionExistente.Money = nuevoMontoPesos;
            transaccionExistente.FechaTransaccion = updateTransaccionDTO.FechaTransaccion;

            _context.Transacciones.Update(transaccionExistente);

            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult> BorrarTransaccionPorId(int id)
        {
            var transaccion = await _context.Transacciones.FindAsync(id);
            if (transaccion == null)
            {
                return NotFound();
            }

            _context.Transacciones.Remove(transaccion);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
