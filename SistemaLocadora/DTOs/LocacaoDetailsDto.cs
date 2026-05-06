namespace SistemaLocadora.DTOs
{
    public class LocacaoDetailsDto
    {
        public int Id { get; set; }

        public int ClienteId { get; set; }
        public string ClienteNome { get; set; } = String.Empty;

        public int VeiculoId { get; set;  }
        public string VeiculoDescricao { get; set; } = String.Empty;

        public DateOnly DataInicio { get; set; }
        public DateOnly DataFim { get; set; }

        public DateOnly? DataDevolucaoReal {  get; set; }

        public decimal multa {  get; set; }
        public decimal ValorTotal { get; set; }

        public bool Ativo { get; set; }

    }
}
