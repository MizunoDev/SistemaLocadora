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

        //Regra 1 -> Cada veículo pode estar locado para um cliente por vez/dia
        public async Task<bool> ExisteConflitoDeLocacaoAsync( 
            int veiculoId,
            DateOnly dataInicio,
            DateOnly dataFim)
        {
            return await _context.Locacoes.AnyAsync(b => // Existe pelo menos UM registro que satisfaça essa condição?
            b.VeiculoId == veiculoId && // Mesmo veículo
            b.Ativo && // Locação ativa
            dataInicio < b.DataFim && // início novo antes do fim antigo
            dataFim > b.DataInicio // fim novo depois do início antigo
            );
        }

    }
}
