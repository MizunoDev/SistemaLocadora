using Microsoft.EntityFrameworkCore;
using SistemaLocadora.Models;

namespace SistemaLocadora.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Veiculo> Veiculos { get; set; }
        public DbSet<CategoriaVeiculo> CategoriasVeiculo { get; set; }
        public DbSet<Locacao> Locacoes { get; set; }
        
    }
}
