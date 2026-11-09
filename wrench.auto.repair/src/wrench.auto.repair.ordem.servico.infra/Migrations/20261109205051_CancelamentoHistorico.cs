using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace wrench.auto.repair.ordem.servico.infra.Migrations
{
    /// <inheritdoc />
    public partial class CancelamentoHistorico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrdemServicoHistorico",
                columns: table => new
                {
                    OrdemServicoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StatusAnterior = table.Column<int>(type: "integer", nullable: true),
                    StatusNovo = table.Column<int>(type: "integer", nullable: false),
                    Motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OcorridoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdemServicoHistorico", x => new { x.OrdemServicoId, x.Id });
                    table.ForeignKey(
                        name: "FK_OrdemServicoHistorico_OrdemServico_OrdemServicoId",
                        column: x => x.OrdemServicoId,
                        principalTable: "OrdemServico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrdemServicoHistorico");
        }
    }
}
