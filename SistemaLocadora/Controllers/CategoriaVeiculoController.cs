using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaLocadora.Data;
using SistemaLocadora.DTOs;
using SistemaLocadora.Models;

namespace SistemaLocadora.Controllers
{

    [Route("api/[controller]")]
    public class CategoriaVeiculoController : ControllerBase
    {


        private readonly AppDbContext _context;

        public CategoriaVeiculoController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoriaVeiculoGetDto>>> GetAll()
        {
            var categorias = await _context.CategoriasVeiculo.ToListAsync();

            var listaDto = categorias.Select(c => new CategoriaVeiculoGetDto
            {
                Id = c.Id,
                Nome = c.Nome,
                ValorDiaSemana = c.ValorDiaSemana,
                ValorFinalSemana = c.ValorFinalSemana,
                IntervaloMinimoDias = c.IntervaloMinimoDias
            }).ToList();

            return Ok(listaDto);

        }

        [HttpGet]
        [Route("{id}")]

        public async Task<ActionResult<CategoriaVeiculoGetDto>> GetById(int id)
        {
            var categoria = await _context.CategoriasVeiculo.FindAsync(id);

            if (categoria == null)
                return NotFound();

            var dto = new CategoriaVeiculoGetDto
            {
                Id = categoria.Id,
                Nome = categoria.Nome,
                ValorDiaSemana = categoria.ValorDiaSemana,
                ValorFinalSemana = categoria.ValorFinalSemana,
                IntervaloMinimoDias = categoria.IntervaloMinimoDias
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult> Create(CategoriaVeiculoCreateDto dto)
        {

            if (dto.IntervaloMinimoDias <= 0)
            {
                return BadRequest("Intervalo mínimo deve ser maior que zero");
            }

            var categoriaveiculo = new CategoriaVeiculo
            {
                Nome = dto.Nome,
                ValorDiaSemana = dto.ValorDiaSemana,
                ValorFinalSemana = dto.ValorFinalSemana,
                IntervaloMinimoDias = dto.IntervaloMinimoDias
            };

            _context.CategoriasVeiculo.Add(categoriaveiculo);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut]
        [Route("{id}")]

        public async Task<IActionResult> Update(int id, CategoriaVeiculoUpdate dto)
        {
            var categoriabanco = await _context.CategoriasVeiculo.FindAsync(id);

            if (categoriabanco == null)
                return NotFound();

            if (dto.IntervaloMinimoDias <= 0)
            {
                return BadRequest("Intervalo mínimo deve ser maior que zero");
            }

            categoriabanco.Nome = dto.Nome;
            categoriabanco.ValorDiaSemana = dto.ValorDiaSemana;
            categoriabanco.ValorFinalSemana = dto.ValorFinalSemana;
            categoriabanco.IntervaloMinimoDias = dto.IntervaloMinimoDias;

            await _context.SaveChangesAsync();

            return Ok();

        }

        [HttpDelete]
        
        public async Task<IActionResult> Delete(int id)
        {
            var categoria = await _context.CategoriasVeiculo.FindAsync(id);

            if (categoria == null)
                return NotFound();

            _context.CategoriasVeiculo.Remove(categoria);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}

