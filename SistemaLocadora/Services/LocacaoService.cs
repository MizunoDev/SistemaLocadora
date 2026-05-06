using SistemaLocadora.Data;
using SistemaLocadora.Models;
using SistemaLocadora.Enums;

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

    }
}