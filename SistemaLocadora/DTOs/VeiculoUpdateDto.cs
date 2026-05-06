using SistemaLocadora.Enums;
using System.ComponentModel.DataAnnotations;

namespace SistemaLocadora.DTOs
{
    public class VeiculoUpdateDto
    {
        [Required]
        public string NumeroRenavam { get; set; } = string.Empty;

        [Required]
        public string Placa { get; set; } = string.Empty;

        [Required]
        public string Chassi { get; set; } = string.Empty;

        [Required]
        public string Marca { get; set; } = string.Empty;

        [Required]
        public string Modelo { get; set; } = string.Empty;

        [Required]
        public int AnoFabricacao { get; set; }

        [Required]
        public SituacaoVeiculo Situacao { get; set; }

        [Required]
        public string UfRegistro { get; set; } = string.Empty;

        [Required]
        public int CategoriaVeiculoId { get; set; }
    }
}
