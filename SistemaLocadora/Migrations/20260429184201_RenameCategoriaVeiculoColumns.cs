using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaLocadora.Migrations
{
    /// <inheritdoc />
    public partial class RenameCategoriaVeiculoColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "valorFinalSemana",
                table: "CategoriasVeiculo",
                newName: "ValorFinalSemana");

            migrationBuilder.RenameColumn(
                name: "valorDiaSemana",
                table: "CategoriasVeiculo",
                newName: "ValorDiaSemana");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ValorFinalSemana",
                table: "CategoriasVeiculo",
                newName: "valorFinalSemana");

            migrationBuilder.RenameColumn(
                name: "ValorDiaSemana",
                table: "CategoriasVeiculo",
                newName: "valorDiaSemana");
        }
    }
}
