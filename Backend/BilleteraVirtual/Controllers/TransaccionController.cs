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
        public async Task<ActionResult> UpdateTransaccion(int id, Transaccion transaccion)
        {
            if(id != transaccion.Id)
            {
                return BadRequest("El Id de la transacción no coincide");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var cliente = await _context.Clientes.FindAsync(transaccion.ClienteId);
            if (cliente == null)
            {
                return BadRequest("Cliente no valido");
            }

            var transaccionExistente = await _context.Transacciones.FindAsync(id);
            if (transaccionExistente == null)
            {
                return NotFound();
            }

            //Se actualizan los campos de la transacción existente con los nuevos valores
            transaccionExistente.CryptoAmount = transaccion.CryptoAmount;
            transaccionExistente.Action = transaccion.Action;
            transaccionExistente.CryptoAmount = transaccion.CryptoAmount;
            transaccionExistente.Money = transaccion.Money;
            transaccionExistente.FechaTransaccion = DateTime.UtcNow; 
            transaccionExistente.ClienteId = transaccion.ClienteId;

            //await _context.SaveChangesAsync();
            _context.Entry(transaccionExistente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Transacciones.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else throw;
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> BorrarTransaccionPorId(int id)
        {
            var traaccion = await _context.Transacciones.FindAsync(id);
            if (traaccion == null)
            {
                return NotFound();
            }

            _context.Transacciones.Remove(traaccion);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
