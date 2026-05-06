using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaLocadora.Data;
using SistemaLocadora.DTOs;
using SistemaLocadora.Enums;
using SistemaLocadora.Models;
using System.Numerics;
using System.Text.RegularExpressions;

namespace SistemaLocadora.Controllers
{
    [Route("api/[controller]")]
    public class ClienteController : ControllerBase
    {

        private readonly AppDbContext _context;

        public ClienteController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteGetDto>>> GetAll()
        {
            var clientes = await _context.Clientes.ToListAsync();

            var listaDto = clientes.Select(c => new ClienteGetDto
            {
                Id = c.Id,
                Nome = c.Nome,
                Cpf = c.Cpf,
                DataNascimento = c.DataNascimento,
                Telefone = c.Telefone,
                Email = c.Email,
                Profissao = c.Profissao
            }).ToList();

            return Ok(listaDto);

        }

        [HttpGet]
        [Route("{id}")]

        public async Task<ActionResult<ClienteGetDto>> GetById(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
                return NotFound();

            var dto = new ClienteGetDto
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Cpf = cliente.Cpf,
                DataNascimento = cliente.DataNascimento,
                Telefone = cliente.Telefone,
                Email = cliente.Email,
                Profissao = cliente.Profissao
            };

            return Ok(dto);

        }

        [HttpPost]
        public async Task<ActionResult> Create(ClienteCreateDto dto)
        {
            var cliente = new Cliente
            {
                Nome = dto.Nome,
                Cpf = dto.Cpf,
                DataNascimento = DateTime.SpecifyKind(dto.DataNascimento, DateTimeKind.Utc),
                Telefone = dto.Telefone,
                Email = dto.Email,
                Profissao = dto.Profissao
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return Ok();

        }

        [HttpPut]
        [Route("{id}")]

        public async Task<IActionResult> Update(int id, ClienteUpdateDto dto)
        {
            var clienteBanco = await _context.Clientes.FindAsync(id);

            if (clienteBanco == null)
                return NotFound();

            clienteBanco.Nome = dto.Nome;
            clienteBanco.Cpf = dto.Cpf;
            clienteBanco.DataNascimento = DateTime.SpecifyKind(dto.DataNascimento, DateTimeKind.Utc);
            clienteBanco.Telefone = dto.Telefone;
            clienteBanco.Email = dto.Email;
            clienteBanco.Profissao = dto.Profissao;

            await _context.SaveChangesAsync();

            return Ok();

        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
                return NotFound();

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}