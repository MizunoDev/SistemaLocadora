using System.ComponentModel.DataAnnotations;

namespace SistemaLocadora.DTOs
{
    public class LocacaoRenovarDto
    {
        [Required]
        public DateOnly NovaDataFim { get; set; }
    }
}
