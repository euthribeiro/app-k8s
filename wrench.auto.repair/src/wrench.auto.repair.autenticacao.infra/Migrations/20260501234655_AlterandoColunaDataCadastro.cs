using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wrench.auto.repair.autenticacao.infra.Migrations
{
    /// <inheritdoc />
    public partial class AlterandoColunaDataCadastro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateCadastro",
                table: "Usuarios",
                newName: "DataCadastro");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DataCadastro",
                table: "Usuarios",
                newName: "DateCadastro");
        }
    }
}
