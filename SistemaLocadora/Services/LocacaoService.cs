using SistemaLocadora.Data;
using SistemaLocadora.Models;
using SistemaLocadora.Enums;
using Microsoft.EntityFrameworkCore;

namespace SistemaLocadora.Services
{
    public class LocacaoService
    {
        private readonly AppDbContext _context;

        public LocacaoService(AppDbContext context)
        {
            _context = context;
        }

        // Verifica se já existe uma locação ativa para o mesmo veículo no período informado.
        // Regra 1: um veículo só pode estar locado para um cliente por vez.
        public async Task<bool> ExisteConflitoDeLocacaoAsync(
            int veiculoId,
            DateOnly dataInicio,
            DateOnly dataFim,
            int? ignorarLocacaoId = null)
        {
            return await _context.Locacoes.AnyAsync(l =>
                l.Id != ignorarLocacaoId &&
                l.VeiculoId == veiculoId &&
                l.Ativo &&
                dataInicio < l.DataFim &&
                dataFim > l.DataInicio
            );
        }

        //Regra 2
        // Garante que o intervalo mínimo entre locações seja respeitado,
        // conforme definido na categoria do veículo
        public async Task<bool> RespeitaIntervaloMinimoAsync(int veiculoId, DateOnly novaDataInicio)
        {
            var veiculo = await _context.Veiculos
                .Include(v => v.CategoriaVeiculo)
                .FirstOrDefaultAsync(v => v.Id == veiculoId);

            if (veiculo == null) return false;

            var ultimaLocacao = await _context.Locacoes
                .Where(l => l.VeiculoId == veiculoId && (l.Ativo || l.DataDevolucaoReal != null))
                .OrderByDescending(l => l.DataDevolucaoReal ?? l.DataFim)
                .FirstOrDefaultAsync();

            if (ultimaLocacao == null) return true;

            var diasIntervalo = veiculo.CategoriaVeiculo.IntervaloMinimoDias;
            var dataMinima = ultimaLocacao.DataFim.AddDays(diasIntervalo);

            return novaDataInicio >= dataMinima;
        }

        // Regra 3
        // Calcula o valor total da locação aplicando valores diferentes para dias de semana e fins de semana.
        // O dia da devolução não é cobrado
        public async Task<decimal> CalcularValorLocacaoAsync(int veiculoId, DateOnly dataInicio, DateOnly dataFim)
        {
            var veiculo = await _context.Veiculos
                .Include(v => v.CategoriaVeiculo)
                .FirstAsync(v => v.Id == veiculoId);

            decimal valorTotal = 0;

            for (var data = dataInicio; data < dataFim; data = data.AddDays(1))
            {
                bool fimDeSemana = data.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
                valorTotal += fimDeSemana
                    ? veiculo.CategoriaVeiculo.ValorFinalSemana
                    : veiculo.CategoriaVeiculo.ValorDiaSemana;
            }

            return valorTotal;
        }

        //Regra 4 -> Validação locação
        // Valida se uma renovação é possivel, respeitando limite de vezes e duração maxima
        public string? ValidarRenovacao(Locacao locacao, DateOnly novaDataFim)
        {
            if (!locacao.Ativo)
                return "Não é possível renovar uma locação finalizada.";

            if (locacao.QuantidadeRenovacoes >= 3)
                return "Limite máximo de renovações atingido.";

            var totalDias = novaDataFim.DayNumber - locacao.DataInicio.DayNumber;
            if (totalDias > 30)
                return "A locação não pode ultrapassar 30 dias.";

            if (novaDataFim <= locacao.DataFim)
                return "A nova data deve ser maior que a data atual.";

            return null;
        }

        //Regra 5
        // Confirma se o pagamento antecipado foi realizado
        public bool PagamentoValido(bool pagamentoConfirmado) => pagamentoConfirmado;

        //Regra 6 -> Multa
        public async Task<decimal> CalcularMultaAsync(Locacao locacao, DateOnly dataDevolucao)
        {
            if (dataDevolucao <= locacao.DataFim) return 0;

            var veiculo = await _context.Veiculos
                .Include(v => v.CategoriaVeiculo)
                .FirstAsync(v => v.Id == locacao.VeiculoId);

            decimal multa = veiculo.CategoriaVeiculo.ValorDiaSemana * 0.5m;

            // Calculo da multa 
            // O cálculo começa a partir do dia seguinte ao término da locação
            // e aplica 120% do valor da diária, variando conforme o dia da semana
            for (var data = locacao.DataFim; data < dataDevolucao; data = data.AddDays(1))
            {
                bool fimDeSemana = data.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
                // Multa diária equivalente a 150% da diária do veículo,
                // variando conforme dia de semana ou final de semana
                multa += fimDeSemana                                   // Ternário
                    ? veiculo.CategoriaVeiculo.ValorFinalSemana * 1.5m // ? VALOR_SE_FOR_FIM_DE_SEMANA
                    : veiculo.CategoriaVeiculo.ValorDiaSemana * 1.5m;  // : VALOR_SE_NAO_FOR_FIM_DE_SEMANA
            }

            return multa;
        }

        //Regra 7 -> Agendar um veículo
        // Verifica se a data de início da locação é válida para agendamento
        public bool DataAgendamentoValida(DateOnly dataInicio)
        {
            var hoje = DateOnly.FromDateTime(DateTime.Today);
            return dataInicio >= hoje;
        }

        // Determina o status atual de uma locação com base na sua situação e datas
        // api/locacao/{id} -> Calcula status da locação
        //Cancelada / Finalizada / Agendada / Andamento

        public StatusLocacaoFiltro CalcularStatus(Locacao locacao, DateOnly hoje)
        {
            if (!locacao.Ativo && locacao.DataDevolucaoReal == null)
                return StatusLocacaoFiltro.Cancelada;

            if (!locacao.Ativo && locacao.DataDevolucaoReal != null)
                return StatusLocacaoFiltro.Finalizada;

            if (locacao.Ativo && locacao.DataInicio > hoje)
                return StatusLocacaoFiltro.Agendada;

            return StatusLocacaoFiltro.Andamento;
        }
    }
}