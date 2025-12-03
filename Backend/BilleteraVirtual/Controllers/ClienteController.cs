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


namespace BilleteraVirtual.Controllers
{
    [Route("cliente")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;

        public ClienteController(AppDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
        {
            return await _context.Clientes.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Cliente>> GetCliente(int id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Transacciones)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cliente == null)
            {
                return NotFound();
            }
            return cliente;
        }

        [HttpPost]
        public async Task<ActionResult<Cliente>> CreateCliente(BilleteraVirtual.DTOs.CrearClienteDTO clienteCreateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var cliente = new Cliente
            {
                Nombre = clienteCreateDTO.Nombre,
                Email = clienteCreateDTO.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(clienteCreateDTO.Password),
                SaldoPesos = 10000
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetClientes), new { id = cliente.Id }, cliente);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(BilleteraVirtual.DTOs.LoginDTO loginDTO)
        {
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Email == loginDTO.Email);
            if (cliente == null || !BCrypt.Net.BCrypt.Verify(loginDTO.Password, cliente.PasswordHash))
            {
                return Unauthorized("Credenciales inválidas.");
            }

            var token = _tokenService.GenerateToken(cliente);

            return Ok(new
            {
                token = token,
                userId = cliente.Id,
                userName = cliente.Nombre
            });
        }   

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Transacciones)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
            {
                return NotFound();
            }

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCliente(int id, BilleteraVirtual.DTOs.UpdateClienteDTO clienteUpdateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }

            cliente.Nombre = clienteUpdateDTO.Nombre;
            cliente.Email = clienteUpdateDTO.Email;
            if (!string.IsNullOrEmpty(clienteUpdateDTO.Password))
            {
                cliente.PasswordHash = BCrypt.Net.BCrypt.HashPassword(clienteUpdateDTO.Password);
            }

            _context.Entry(cliente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Clientes.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else throw;
            }
        }

        [HttpPatch("{id}/saldo")]
        public async Task<IActionResult> UpdateSaldo(int id, BilleteraVirtual.DTOs.ActualizarSaldoPesosDTO actualizarSaldoPesos)
        {
            if (!ModelState.IsValid)
            { 
                return BadRequest(ModelState);
            }

            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
            {
                return NotFound();
            }

            cliente.SaldoPesos += actualizarSaldoPesos.SaldoPesos;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Clientes.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else throw;
            }
            return NoContent();
        }
    }
}
