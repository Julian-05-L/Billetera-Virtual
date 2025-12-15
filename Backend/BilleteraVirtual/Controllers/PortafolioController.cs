using BilleteraVirtual.Models;
using BilleteraVirtual.DTOs;
using BilleteraVirtual.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Writers;
using System.Security.Cryptography;
using System.Text.Json;
using System.Runtime.InteropServices.Marshalling;
using System.Diagnostics;
using System.Data;
using BCrypt.Net;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Linq;


namespace BilleteraVirtual.Controllers
{
    [Route("portafolio")]
    [ApiController]
    public class PortafolioController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _http;

        public PortafolioController(AppDbContext context)
        {
            _context = context;
            _http = new HttpClient();
        }

        [HttpGet("{clienteId}")]
        public async Task<ActionResult> ObtenerPortafolio(int clienteId)
        {
            // se obtienen todas las transaccciones del cliente
            var transacciones = await _context.Transacciones
                .Where(t => t.ClienteId == clienteId)
                .ToListAsync();

            if (!transacciones.Any()) 
            {
                return Ok(new
                {
                    distribucion = new List<object>(),
                    valorTotalARS = 0
                });
            }

            // se calculan los balances por criptomoneda
            var balances = transacciones 
                .GroupBy(t => t.CryptoCode)
                .Select(g => new { 
                    Crypto = g.Key,
                    Cantidad = g.Where(x => x.Action == "purchase").Sum(x => x.CryptoAmount)
                                    - g.Where(x => x.Action == "sale").Sum(x => x.CryptoAmount)
                })
                .Where(b => b.Cantidad > 0)
                .ToList();

            decimal valorTotalARS = 0;
            var distribucion = new List<object>();

            // precio actual de cada criptomoneda y calculo del valor total
            foreach (var balance in balances)
            {
                decimal precioARS;
                try
                {
                    var url = $"https://criptoya.com/api/argenbtc/{balance.Crypto}/ars";
                    var response = await _http.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    var json = await response.Content.ReadAsStringAsync();
                    using var jsonDoc = JsonDocument.Parse(json);
                    precioARS = jsonDoc.RootElement.GetProperty("totalAsk").GetDecimal();
                }
                catch
                {
                    return BadRequest($"Error al obtener el precio de la criptomoneda {balance.Crypto}.");
                }
                var valorARS = balance.Cantidad * precioARS;
                valorTotalARS += valorARS;
                distribucion.Add(new
                {
                    cryptoCode = balance.Crypto,
                    cantidad = balance.Cantidad,
                    valorARS = valorARS
                });
            }
            return Ok(new
            {
                distribucion = distribucion,
                valorTotalARS = valorTotalARS
            });
        }
    }
}