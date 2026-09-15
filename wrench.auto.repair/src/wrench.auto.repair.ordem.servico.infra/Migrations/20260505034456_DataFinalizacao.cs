using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wrench.auto.repair.ordem.servico.infra.Migrations
{
    /// <inheritdoc />
    public partial class DataFinalizacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataFinalizacao",
                table: "OrdemServico",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataFinalizacao",
                table: "OrdemServico");
        }
    }
}
