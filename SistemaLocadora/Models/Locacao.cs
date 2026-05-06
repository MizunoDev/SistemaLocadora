namespace SistemaLocadora.Models
{
    public class Locacao
    {
        public int Id { get; set; }
        public int ClienteId {  get; set; }
        public Cliente Cliente { get; set; } = null!;
        public int VeiculoId { get; set; }
        public Veiculo Veiculo { get; set; } = null!;
        public DateOnly DataInicio { get; set; } 
        public DateOnly DataFim { get; set; }
        public DateOnly? DataDevolucaoReal { get; set; }
        public decimal Multa { get; set; }
        public Decimal ValorTotal { get; set; }
        public int QuantidadeRenovacoes { get; set; }
        public bool Ativo { get; set; }
    }
}
