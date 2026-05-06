using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaLocadora.Migrations
{
    /// <inheritdoc />
    public partial class AddLocacaoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DataDevolucaoReal",
                table: "Locacoes",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Multa",
                table: "Locacoes",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "QuantidadeRenovacoes",
                table: "Locacoes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataDevolucaoReal",
                table: "Locacoes");

            migrationBuilder.DropColumn(
                name: "Multa",
                table: "Locacoes");

            migrationBuilder.DropColumn(
                name: "QuantidadeRenovacoes",
                table: "Locacoes");
        }
    }
}
