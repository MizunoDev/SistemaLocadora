using System.ComponentModel.DataAnnotations;

namespace SistemaLocadora.DTOs
{
    public class ClienteUpdateDto
    {
        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required]
        public string Cpf { get; set; } = string.Empty;

        [Required]
        public DateTime DataNascimento { get; set; }

        [Required]
        public string Telefone { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Profissao { get; set; } = string.Empty;
    }
}
