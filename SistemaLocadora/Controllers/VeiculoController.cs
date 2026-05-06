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
    public class VeiculoController : ControllerBase
    {

        private readonly AppDbContext _context;

        public VeiculoController(AppDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<VeiculoGetDto>>> GetAll()
        {
            var veiculos = await _context.Veiculos
                .Include(v => v.CategoriaVeiculo)
                .ToListAsync();

            var listaDto = veiculos.Select(v => new VeiculoGetDto
            {
                Id = v.Id,
                NumeroRenavam = v.NumeroRenavam,
                Placa = v.Placa,
                Chassi = v.Chassi,
                Marca = v.Marca,
                Modelo = v.Modelo,
                AnoFabricacao = v.AnoFabricacao,
                Situacao = v.Situacao,
                UfRegistro = v.UfRegistro,
                CategoriaVeiculoId = v.CategoriaVeiculoId,
                CategoriaVeiculoDescricao = v.CategoriaVeiculo.Nome
            }).ToList();

            return Ok(listaDto);
        }


        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<IEnumerable<VeiculoGetDto>>> GetById(int id)
        {
            var veiculo = await _context.Veiculos
                .Include(v => v.CategoriaVeiculo)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (veiculo == null)
                return NotFound();

            var dto = new VeiculoGetDto
            {
                Id = veiculo.Id,
                NumeroRenavam = veiculo.NumeroRenavam,
                Placa = veiculo.Placa,
                Chassi = veiculo.Chassi,
                Marca = veiculo.Marca,
                Modelo = veiculo.Modelo,
                AnoFabricacao = veiculo.AnoFabricacao,
                Situacao = veiculo.Situacao,
                UfRegistro = veiculo.UfRegistro,
                CategoriaVeiculoId = veiculo.CategoriaVeiculoId,
                CategoriaVeiculoDescricao = veiculo.CategoriaVeiculo.Nome
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult> Create(VeiculoCreateDto dto)
        {
            var veiculo = new Veiculo
            {

                NumeroRenavam = dto.NumeroRenavam,
                Placa = dto.Placa,
                Chassi = dto.Chassi,
                Marca = dto.Marca,
                Modelo = dto.Modelo,
                AnoFabricacao = dto.AnoFabricacao,
                Situacao = dto.Situacao,
                UfRegistro = dto.UfRegistro,
                CategoriaVeiculoId = dto.CategoriaVeiculoId

            };

            _context.Veiculos.Add(veiculo);
            await _context.SaveChangesAsync();

            return Ok();


            //_context.Veiculos -> é o endereço/representação da tabela Veiculos dentro desse mensageiro.

        }

        [HttpPut]
        [Route("{id}")]

        public async Task<IActionResult> Update(int id, VeiculoUpdateDto dto)
        {
            var veiculoBanco = await _context.Veiculos.FindAsync(id);

            if (veiculoBanco == null)
                return NotFound();

            veiculoBanco.NumeroRenavam = dto.NumeroRenavam;
            veiculoBanco.Placa = dto.Placa;
            veiculoBanco.Chassi = dto.Chassi;
            veiculoBanco.Marca = dto.Marca;
            veiculoBanco.Modelo = dto.Modelo;
            veiculoBanco.AnoFabricacao = dto.AnoFabricacao;
            veiculoBanco.Situacao = dto.Situacao;
            veiculoBanco.UfRegistro = dto.UfRegistro;
            veiculoBanco.CategoriaVeiculoId = dto.CategoriaVeiculoId;

            await _context.SaveChangesAsync();

            return Ok();

        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var veiculo = await _context.Veiculos.FindAsync(id);

            if (veiculo == null)
                return NotFound();

            _context.Veiculos.Remove(veiculo);
            await _context.SaveChangesAsync();

            return NoContent();

        }

    }
}

