using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wrench.auto.repair.ordem.servico.infra.Migrations
{
    /// <inheritdoc />
    public partial class IndicesConsultaOrdemServico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_OrdemServico_ClienteId",
                table: "OrdemServico",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdemServico_Status_DataCriacao",
                table: "OrdemServico",
                columns: new[] { "Status", "DataCriacao" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrdemServico_ClienteId",
                table: "OrdemServico");

            migrationBuilder.DropIndex(
                name: "IX_OrdemServico_Status_DataCriacao",
                table: "OrdemServico");
        }
    }
}
