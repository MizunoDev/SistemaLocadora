using System.ComponentModel.DataAnnotations;

namespace SistemaLocadora.DTOs
{
    public class LocacaoCreateDto
    {

        [Required]
        public int ClienteId { get; set; }

        [Required]
        public int VeiculoId { get; set; }

        [Required]
        public DateOnly DataInicio { get; set; }

        [Required]
        public DateOnly DataFim {  get; set; }

        [Required]
        public bool PagamentoConfirmado { get; set; }
    }
}
