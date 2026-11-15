using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wrench.auto.repair.ordem.servico.infra.Migrations
{
    /// <inheritdoc />
    public partial class SagaOrdemServico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SagaInbox",
                columns: table => new
                {
                    MessageId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Tipo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RecebidaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SagaInbox", x => x.MessageId);
                });

            migrationBuilder.CreateTable(
                name: "SagaOrdemServico",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrdemServicoId = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    Etapa = table.Column<int>(type: "integer", nullable: false),
                    PrazoEtapa = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MotivoDesfecho = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OrcamentoId = table.Column<Guid>(type: "uuid", nullable: true),
                    PagamentoId = table.Column<Guid>(type: "uuid", nullable: true),
                    ValorAprovado = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataDesfecho = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SagaOrdemServico", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SagaOutbox",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrdemServicoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Traceparent = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CriadaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublicadaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Tentativas = table.Column<int>(type: "integer", nullable: false),
                    UltimoErro = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SagaOutbox", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SagaTransicao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SagaOrdemServicoId = table.Column<Guid>(type: "uuid", nullable: false),
                    EstadoAnterior = table.Column<int>(type: "integer", nullable: true),
                    EstadoNovo = table.Column<int>(type: "integer", nullable: false),
                    Etapa = table.Column<int>(type: "integer", nullable: false),
                    Gatilho = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OcorridaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SagaTransicao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SagaTransicao_SagaOrdemServico_SagaOrdemServicoId",
                        column: x => x.SagaOrdemServicoId,
                        principalTable: "SagaOrdemServico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SagaOrdemServico_Estado_PrazoEtapa",
                table: "SagaOrdemServico",
                columns: new[] { "Estado", "PrazoEtapa" });

            migrationBuilder.CreateIndex(
                name: "IX_SagaOrdemServico_OrdemServicoId",
                table: "SagaOrdemServico",
                column: "OrdemServicoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SagaOutbox_PublicadaEm_CriadaEm",
                table: "SagaOutbox",
                columns: new[] { "PublicadaEm", "CriadaEm" });

            migrationBuilder.CreateIndex(
                name: "IX_SagaTransicao_SagaOrdemServicoId",
                table: "SagaTransicao",
                column: "SagaOrdemServicoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SagaInbox");

            migrationBuilder.DropTable(
                name: "SagaOutbox");

            migrationBuilder.DropTable(
                name: "SagaTransicao");

            migrationBuilder.DropTable(
                name: "SagaOrdemServico");
        }
    }
}
