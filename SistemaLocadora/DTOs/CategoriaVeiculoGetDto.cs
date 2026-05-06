using System.ComponentModel.DataAnnotations;

namespace SistemaLocadora.DTOs
{
    public class CategoriaVeiculoGetDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal ValorDiaSemana { get; set; }
        public decimal ValorFinalSemana { get; set; }
        public int IntervaloMinimoDias { get; set; }
    }
}

