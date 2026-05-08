using SistemaLocadora.Enums;

namespace SistemaLocadora.Models
{
    public class Veiculo
    {
        public int Id { get; set; }

        public string NumeroRenavam { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public string Chassi { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int AnoFabricacao { get; set; }
        public SituacaoVeiculo Situacao { get; set; } //enum
        public string UfRegistro { get; set; } = string.Empty;
        public int CategoriaVeiculoId { get; set; }
        public CategoriaVeiculo CategoriaVeiculo { get; set; } = null!;
        public List<Locacao> Locacoes { get; set; } = new();


    }
}
