namespace SistemaLocadora.DTOs
{
    public class LocacaoGetDto
    {

        public int Id { get; set; }

        public int ClienteId { get; set; }
        public string ClienteNome { get; set; } = String.Empty;

        public int VeiculoId { get; set; }

        public DateOnly DataInicio { get; set; }
        public DateOnly DataFim { get; set; }

        public DateOnly? DataDevolucaoReal { get; set; } // ? pode retornar null

        public decimal multa { get; set; }
        public decimal ValorTotal { get; set; }

        public StatusLocacaoFiltro Status { get; set; }

    }
}
