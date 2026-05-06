namespace SistemaLocadora.Models
{
    public class CategoriaVeiculo
    {

        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal ValorDiaSemana { get; set; }
        public decimal ValorFinalSemana { get; set; }
        public List<Veiculo> veiculos { get; set; } = new();
        public int IntervaloMinimoDias { get; set; }


    }
}
