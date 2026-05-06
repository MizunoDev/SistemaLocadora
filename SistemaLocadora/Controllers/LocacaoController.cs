using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaLocadora.Data;
using SistemaLocadora.DTOs;
using SistemaLocadora.Models;
using SistemaLocadora.Services;
using System.Text.RegularExpressions;

namespace SistemaLocadora.Controllers
{
    [Route("api/[controller]")]
    public class LocacaoController : ControllerBase
    {


        private readonly AppDbContext _context;
        private readonly LocacaoService _locacaoService;

        public LocacaoController(AppDbContext context, LocacaoService locacaoService)
        {
            _context = context;
            _locacaoService = locacaoService;
        }



        [HttpPost]
        public async Task<ActionResult> Create(LocacaoCreateDto dto)
        {

            //Validação pra saber se o cliente existe 
            var clienteExiste = await _context.Clientes.AnyAsync(
                    b => b.Id == dto.ClienteId);

            if (!clienteExiste)
            {
                return BadRequest("Cliente não encontrado");

            }

            //Valida pra saber se o veículo existe

            var veiculoExiste = await _context.Veiculos.AnyAsync( // AnyAsync -> retorna um bool ( V F )
                v => v.Id == dto.VeiculoId);

            if (!veiculoExiste)
            {
                return BadRequest("Veículo não encontrado");
            }

            //Se pagamentoConfirmado == false
            //Regra 5 - O pagamento é antecipado
            if (!dto.PagamentoConfirmado)
                return BadRequest("O pagamento deve ser confirmado antes de criar a locação");


            if (dto.DataInicio >= dto.DataFim)
            {
                return BadRequest("A data de início deve ser anterior à data de fim");
            }


            //Regra 1 - Cada veículo pode estar locado para um cliente por vez/dia
            //Regra 7 - Deve haver como agendar um veículo (ex: quero locar o veículo tal pra daqui a 30 dias)


            var existeConflito = await _locacaoService
                .ExisteConflitoDeLocacaoAsync(
                //Qual veículo o cliente quer alugar
                dto.VeiculoId,
                //Quando a locação começa
                dto.DataInicio,
                //Quando termina
                dto.DataFim

            );

            if (existeConflito)
            {
                return BadRequest("Este veículo já está locado nesse perído");
            }

            var veiculo = await _context.Veiculos
                    .Include(v => v.CategoriaVeiculo)
                    .FirstOrDefaultAsync(v => v.Id == dto.VeiculoId);

            if (veiculo == null)
            {
                return NotFound("Veículo não encontrado");
            }

            //Regra 2 - Deve haver um intervalo de um dia entre a entrega do veículo e a próxima locação
            //Regra 7 - Deve haver como agendar um veículo (ex: quero locar o veículo tal pra daqui a 30 dias)

            var ultimaLocacao = await _context.Locacoes
                .Where(b =>
                    b.VeiculoId == dto.VeiculoId &&
                    (b.Ativo || b.DataDevolucaoReal != null) // ✅ ignora canceladas
                )
                .OrderByDescending(b => b.DataDevolucaoReal ?? b.DataFim)
                .FirstOrDefaultAsync();

            if (ultimaLocacao != null)
            {

                var diasIntervalo = veiculo.CategoriaVeiculo.IntervaloMinimoDias;
                var dataMinima = ultimaLocacao.DataFim.AddDays(diasIntervalo);

                if (dto.DataInicio < dataMinima)
                {
                    return BadRequest($"Este veículo só pode ser locado a partir de {dataMinima:yyyy-MM-dd}");
                }
            }

            // Regra 3 - O preço da locação dos veículos pode variar de acordo com o dia (feriado e final de semana
            // a diária é mais cara)

            decimal valorTotal = 0;

            for (var data = dto.DataInicio; data < dto.DataFim; data = data.AddDays(1))
            {
                if (data.DayOfWeek == DayOfWeek.Saturday ||
                   data.DayOfWeek == DayOfWeek.Sunday)
                {
                    valorTotal += veiculo.CategoriaVeiculo.ValorFinalSemana;
                }
                else
                {
                    valorTotal += veiculo.CategoriaVeiculo.ValorDiaSemana;
                }
            }

            //--------------------------//

            //Regra 4 parte 2 - não ultrapasse o total de 30 dias durante a primeira locação:

            var totalDias = dto.DataFim.DayNumber - dto.DataInicio.DayNumber;
            if (totalDias > 30)
            {
                return BadRequest("A locação não pode ultrapassar 30 dias");
            }


            //-------------------------//

            var locacao = new Locacao
            {
                ClienteId = dto.ClienteId,
                VeiculoId = dto.VeiculoId,
                DataInicio = dto.DataInicio,
                DataFim = dto.DataFim,
                ValorTotal = valorTotal,
                Ativo = true,
                QuantidadeRenovacoes = 0,
                Multa = 0,
            };

            _context.Locacoes.Add(locacao);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Create), new { id = locacao.Id }, null);
        }

        //Regra 4 Após a locação feita o cliente pode renovar a locação até 3x desde que não ultrapasse o total de 30 dias

        [HttpPost("{id}/renovar")]
        public async Task<ActionResult> Renovar(int id, LocacaoRenovarDto dto)
        {

            var locacao = await _context.Locacoes
                .FirstOrDefaultAsync(b => b.Id == id);

            if (locacao == null)
            {
                return NotFound("Locação não encontrada");
            }

            if (!locacao.Ativo)
            {
                return BadRequest("Não é possível renovar uma locação já finalizada");
            }

            if (locacao.QuantidadeRenovacoes >= 3)
            {
                return BadRequest("Limite máximo de renovações atingidas. Máximo permitido: 3");
            }

            var totalDias = dto.NovaDataFim.DayNumber - locacao.DataInicio.DayNumber;
            if (totalDias > 30)
            {
                return BadRequest("A locação não pode ultrapassar 30 dias");
            }

            var veiculo = await _context.Veiculos
                .Include(v => v.CategoriaVeiculo)
                .FirstAsync(v => v.Id == locacao.VeiculoId);


            // Guarda a data antiga
            var dataFimAntiga = locacao.DataFim;

            // Calcula o valor adicional
            decimal valorAdicional = 0;

            for (var data = dataFimAntiga; data < dto.NovaDataFim; data = data.AddDays(1))
            {
                if (data.DayOfWeek == DayOfWeek.Saturday ||
                    data.DayOfWeek == DayOfWeek.Sunday)
                {
                    valorAdicional += veiculo.CategoriaVeiculo.ValorFinalSemana;
                }
                else
                {
                    valorAdicional += veiculo.CategoriaVeiculo.ValorDiaSemana;
                }
            }

            //-----------------------------------------------------------------//

            //Atualiza valores
            locacao.ValorTotal += valorAdicional;
            locacao.DataFim = dto.NovaDataFim;
            locacao.QuantidadeRenovacoes++;

            await _context.SaveChangesAsync();

            return Ok();
        }

        //Regra 6 Caso o veículo seja entregue com atraso, uma multa será cobrada de acordo com o tipo de veículo e uma multa diária

        [HttpPost("{id}/finalizar")]
        public async Task<ActionResult> Finalizar(int id)
        {
            var locacao = await _context.Locacoes
                .FirstOrDefaultAsync(b => b.Id == id);

            if (locacao == null)
                return NotFound("Locacão não encontrada");

            if (!locacao.Ativo)
                return BadRequest("Locação já finalizada");

            var hoje = DateOnly.FromDateTime(DateTime.Today);

            if (locacao.DataInicio > hoje)
            {
                return BadRequest("Não é possível finalizar uma locação que ainda não foi iniciada.");
            }

            var dataDevolucao = DateOnly.FromDateTime(DateTime.Today);
            locacao.DataDevolucaoReal = dataDevolucao;

            decimal multa = 0;

            if (dataDevolucao > locacao.DataFim)
            {
                var veiculo = await _context.Veiculos
                    .Include(b => b.CategoriaVeiculo)
                    .FirstAsync(b => b.Id == locacao.VeiculoId);

                for (var data = locacao.DataFim; data < dataDevolucao; data = data.AddDays(1))
                {
                    if (data.DayOfWeek == DayOfWeek.Saturday ||
                        data.DayOfWeek == DayOfWeek.Sunday)
                    {
                        multa += veiculo.CategoriaVeiculo.ValorFinalSemana * 1.2m; // Valor da diária + 20% multa de atraso
                    }
                    else
                    {
                        multa += veiculo.CategoriaVeiculo.ValorDiaSemana * 1.2m;
                    }
                }
            }

            locacao.Multa = multa;
            locacao.Ativo = false;

            await _context.SaveChangesAsync();

            return Ok(new { Multa = multa });

        }


        [HttpPost("{id}/cancelar")]

        public async Task<ActionResult> Cancelar(int id)
        {
            var locacao = await _context.Locacoes
                .FirstOrDefaultAsync(l => l.Id == id);

            if (locacao == null)
                return NotFound("Locação não encontrada");

            if (!locacao.Ativo)
                return BadRequest("Essa locação já está finalizada");

            var hoje = DateOnly.FromDateTime(DateTime.Today);

            if (locacao.DataInicio <= hoje)
                return BadRequest("Não é possível iniciar uma locação que já iniciou");

            locacao.Ativo = false;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LocacaoGetDto>>> GetAll(
                StatusLocacaoFiltro? status = null,
                int? clienteId = null,
                int? veiculoId = null)
        {
            var hoje = DateOnly.FromDateTime(DateTime.Today);

            var query = _context.Locacoes
                .Include(l => l.Cliente)
                .AsQueryable();

            if (clienteId.HasValue)
                query = query.Where(l => l.ClienteId == clienteId.Value);

            if (veiculoId.HasValue)
                query = query.Where(l => l.VeiculoId == veiculoId.Value);

            var locacoes = await query
                .OrderByDescending(l => l.DataInicio)
                .Select(l => new
                {
                    Locacao = l,
                    Dto = new LocacaoGetDto
                    {
                        Id = l.Id,
                        ClienteId = l.ClienteId,
                        ClienteNome = l.Cliente.Nome,
                        VeiculoId = l.VeiculoId,
                        DataInicio = l.DataInicio,
                        DataFim = l.DataFim,
                        DataDevolucaoReal = l.DataDevolucaoReal,
                        ValorTotal = l.ValorTotal,
                        multa = l.Multa
                    }
                })
                .ToListAsync();

            foreach (var item in locacoes)
            {
                item.Dto.Status = _locacaoService.CalcularStatus(item.Locacao, hoje);
            }

            var resultado = locacoes.Select(x => x.Dto).ToList();

            if (status.HasValue)
            {
                resultado = resultado
                    .Where(d => d.Status == status.Value)
                    .ToList();
            }

            return Ok(resultado);
        }

        [HttpGet]
        [Route("{id}")]

        public async Task<ActionResult<LocacaoGetDto>> GetById(int id)
        {
            var hoje = DateOnly.FromDateTime(DateTime.Today);

            var locacao = await _context.Locacoes
                .Include(l => l.Cliente)
                .Include(l => l.Veiculo)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (locacao == null)
                return NotFound();

            var status =
                    !locacao.Ativo && locacao.DataDevolucaoReal == null
                        ? StatusLocacaoFiltro.Cancelada
                    : !locacao.Ativo && locacao.DataDevolucaoReal != null
                        ? StatusLocacaoFiltro.Finalizada
                    : locacao.Ativo && locacao.DataInicio > hoje
                        ? StatusLocacaoFiltro.Agendada
                    : StatusLocacaoFiltro.Andamento;


            var dto = new LocacaoGetDto
            {
                Id = locacao.Id,
                ClienteId = locacao.ClienteId,
                ClienteNome = locacao.Cliente.Nome,
                VeiculoId = locacao.VeiculoId,
                DataInicio = locacao.DataInicio,
                DataFim = locacao.DataFim,
                DataDevolucaoReal = locacao.DataDevolucaoReal,
                multa = locacao.Multa,
                ValorTotal = locacao.ValorTotal,
                Status = _locacaoService.CalcularStatus(locacao, hoje)
            };


            return Ok(dto);
        }


    }
}