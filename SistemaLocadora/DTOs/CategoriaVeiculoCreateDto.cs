using System.ComponentModel.DataAnnotations;

namespace SistemaLocadora.DTOs
{
    public class CategoriaVeiculoCreateDto
    {
        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required]
        public decimal ValorDiaSemana { get; set; }

        [Required]
        public decimal ValorFinalSemana { get; set; }

        [Required]
        public int IntervaloMinimoDias { get; set;  }
    }
}
