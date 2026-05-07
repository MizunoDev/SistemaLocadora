using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaLocadora.Data;
using SistemaLocadora.DTOs;
using SistemaLocadora.Enums;
using SistemaLocadora.Models;
using SistemaLocadora.Services;

namespace SistemaLocadora.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocacaoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly LocacaoService _locacaoService;

        public LocacaoController(
            AppDbContext context,
            LocacaoService locacaoService)
        {
            _context = context;
            _locacaoService = locacaoService;
        }

        // Criar Locação 

        [HttpPost]
        public async Task<ActionResult> Create(LocacaoCreateDto dto)
        {

            var clienteExiste = await _context.Clientes
                .AnyAsync(c => c.Id == dto.ClienteId);

            if (!clienteExiste)
                return BadRequest("Cliente não encontrado");


            var veiculoExiste = await _context.Veiculos
                .AnyAsync(v => v.Id == dto.VeiculoId);

            if (!veiculoExiste)
                return BadRequest("Veículo não encontrado");

            // Regra 5 -> O pagamento antecipado é obrigatório para criar a locação
            if (!_locacaoService.PagamentoValido(
                dto.PagamentoConfirmado))
            {
                return BadRequest(
                    "O pagamento deve ser confirmado");
            }

            // Validação básica: a data de início precisa ser anterior à data final
            if (dto.DataInicio >= dto.DataFim)
            {
                return BadRequest(
                    "A data de início deve ser menor que a data fim"); 
            }

            // Regra 7 -> Agendar veículo

            if (!_locacaoService
                .DataAgendamentoValida(dto.DataInicio))
            {
                return BadRequest(
                    "Não é permitido criar locações no passado"); 
            }

            // Regra 1 -> Conflito de locação

            var conflito = await _locacaoService
                .ExisteConflitoDeLocacaoAsync(
                    dto.VeiculoId,
                    dto.DataInicio,
                    dto.DataFim);

            if (conflito)
            {
                return BadRequest(
                    "Este veículo já está locado nesse período"); //Post/api/Locacao
            }

            // Regra 2 -> Intervalo entre Locações
            //O Veículo só pode ser locado após o período de limpeza/manutenção
            //que varia de acordo com a categoria:
            //Economico -> 1 dia
            //SUV       -> 2 dias
            //Luxo      -> 3 dias

            var respeitaIntervalo = await _locacaoService
                .RespeitaIntervaloMinimoAsync(
                    dto.VeiculoId,
                    dto.DataInicio);

            if (!respeitaIntervalo)
            {
                return BadRequest(
                    "O veículo ainda está no período de limpeza/manutenção"); 
            }                                                                                                                                   


            // Regra 3 -> Valor da Locação

            var valorTotal = await _locacaoService
                .CalcularValorLocacaoAsync(
                    dto.VeiculoId,
                    dto.DataInicio,
                    dto.DataFim);

            var locacao = new Locacao
            {
                ClienteId = dto.ClienteId,
                VeiculoId = dto.VeiculoId,
                DataInicio = dto.DataInicio,
                DataFim = dto.DataFim,
                ValorTotal = valorTotal,
                QuantidadeRenovacoes = 0,
                Multa = 0,
                Ativo = true
            };

            _context.Locacoes.Add(locacao);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = locacao.Id },
                null);
        }

        // Renovar

        [HttpPost("{id}/renovar")]
        public async Task<ActionResult> Renovar(
            int id,
            LocacaoRenovarDto dto)
        {
            var locacao = await _context.Locacoes
                .FirstOrDefaultAsync(l => l.Id == id);

            if (locacao == null)
                return NotFound("Locação não encontrada");

            // Regra 4

            var erro = _locacaoService
                .ValidarRenovacao(
                    locacao,
                    dto.NovaDataFim);

            if (erro != null)
                return BadRequest(erro);

            // Conflito -> Regra 1

            var conflito = await _locacaoService
                .ExisteConflitoDeLocacaoAsync(
                    locacao.VeiculoId,
                    locacao.DataFim,
                    dto.NovaDataFim,
                    locacao.Id);

            if (conflito)
            {
                return BadRequest(
                    "Já existe outra locação agendada");
            }

            // Valor adicional

            var valorAdicional =
                await _locacaoService
                    .CalcularValorLocacaoAsync(
                        locacao.VeiculoId,
                        locacao.DataFim,
                        dto.NovaDataFim);

            locacao.DataFim = dto.NovaDataFim;
            locacao.QuantidadeRenovacoes++;
            locacao.ValorTotal += valorAdicional;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // Finalizar

        [HttpPost("{id}/finalizar")]
        public async Task<ActionResult> Finalizar(int id)
        {
            var locacao = await _context.Locacoes
                .FirstOrDefaultAsync(l => l.Id == id);

            if (locacao == null)
                return NotFound("Locação não encontrada");

            if (!locacao.Ativo)
            {
                return BadRequest(
                    "Locação já finalizada");
            }

            var hoje =
                DateOnly.FromDateTime(DateTime.Today);

            if (locacao.DataInicio > hoje)
            {
                return BadRequest(
                    "Não é possível finalizar uma locação agendada");
            }

            locacao.DataDevolucaoReal = hoje;

            // REGRA 6
            // MULTA

            var multa = await _locacaoService
                .CalcularMultaAsync(
                    locacao,
                    hoje);

            locacao.Multa = multa;
            locacao.Ativo = false;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Multa = multa
            });
        }

        // Cancelar -> api/Locacao/{id}/cancelar

        [HttpPost("{id}/cancelar")]
        public async Task<ActionResult> Cancelar(int id)
        {
            var locacao = await _context.Locacoes
                .FirstOrDefaultAsync(l => l.Id == id);

            if (locacao == null)
                return NotFound("Locação não encontrada");

            if (!locacao.Ativo)
            {
                return BadRequest(
                    "Essa locação já está finalizada");
            }

            var hoje =
                DateOnly.FromDateTime(DateTime.Today);

            if (locacao.DataInicio <= hoje)
            {
                return BadRequest(
                    "Não é possível cancelar uma locação já iniciada");
            }

            locacao.Ativo = false;

            await _context.SaveChangesAsync();

            return Ok();
        }


        // Listar todos -> Locacao
        //Lista locações com filtros opcionais e calcula status dinamicamente
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LocacaoGetDto>>> GetAll(
            StatusLocacaoFiltro? status = null,
            int? clienteId = null,
            int? veiculoId = null)
        {
            //Data atual usada para calcular o status da locação
            var hoje =
                DateOnly.FromDateTime(DateTime.Today);

            var query = _context.Locacoes
                .Include(l => l.Cliente)
                .AsQueryable();

            //Filtros opcionais aplicados apenas se informados
            if (clienteId.HasValue)
            {
                query = query
                    .Where(l => l.ClienteId == clienteId.Value);
            }

            if (veiculoId.HasValue)
            {
                query = query
                    .Where(l => l.VeiculoId == veiculoId.Value);
            }
            
            var locacoes = await query
                .OrderByDescending(l => l.DataInicio)
                .ToListAsync();

            //O Status não é salvo no banco. É calculado dinamicamente
            var resultado = locacoes
                .Select(l => new LocacaoGetDto
                {
                    Id = l.Id,
                    ClienteId = l.ClienteId,
                    ClienteNome = l.Cliente.Nome,
                    VeiculoId = l.VeiculoId,
                    DataInicio = l.DataInicio,
                    DataFim = l.DataFim,
                    DataDevolucaoReal = l.DataDevolucaoReal,
                    multa = l.Multa,
                    ValorTotal = l.ValorTotal,
                    Status = _locacaoService
                        .CalcularStatus(l, hoje)
                })
                .ToList();

            //Filtro por status ocorre após o mapeamento,
            //pos o status é calculado no service
            if (status.HasValue)
            {
                resultado = resultado
                    .Where(l => l.Status == status.Value)
                    .ToList();
            }

            return Ok(resultado);
        }

        // Buscar por ID -> Locacao

        [HttpGet("{id}")]
        public async Task<ActionResult<LocacaoGetDto>> GetById(int id)
        {
            var hoje =
                DateOnly.FromDateTime(DateTime.Today);

            var locacao = await _context.Locacoes
                .Include(l => l.Cliente)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (locacao == null)
                return NotFound();

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
                Status = _locacaoService
                    .CalcularStatus(locacao, hoje)
            };

            return Ok(dto);
        }
    }
}