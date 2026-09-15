using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using wrench.auto.repair.cadastro.infra;
using wrench.web.api.integration.tests.Base;

namespace wrench.web.api.integration.tests.Tests
{
    /// <summary>
    /// Contrato de banco com o repositório <c>lambda-auth</c>. A function de autenticação por CPF lê, somente
    /// leitura e com GRANT restrito a estas colunas, as tabelas mantidas pelas migrations desta aplicação.
    /// Renomear, remover ou mudar o tipo de qualquer coluna listada quebra a autenticação em produção; este
    /// teste falha antes disso. Alterar o contrato exige mudar em conjunto o <c>DbContext</c> da Lambda e os
    /// grants do repositório <c>infra-db</c>.
    /// </summary>
    [Collection("SharedContainer")]
    public class ContratoLambdaAuthTest(IntegrationTestFactory integrationTestFactory)
    {
        private const string ConsultaTipoColuna = """
            SELECT data_type
            FROM information_schema.columns
            WHERE table_schema = 'public'
              AND table_name = @tabela
              AND column_name = @coluna
            """;

        public static TheoryData<string, string, string> ColunasDoContrato => new()
        {
            { "Clientes", "Id", "uuid" },
            { "Clientes", "Documento", "text" },
            { "Clientes", "Email", "text" },
            { "Usuarios", "Id", "uuid" },
            { "Usuarios", "Email", "text" },
            { "Usuarios", "PerfilId", "uuid" },
            { "Usuarios", "Ativo", "boolean" },
            { "Perfis", "Id", "uuid" },
            { "Perfis", "Nome", "text" }
        };

        [Theory(DisplayName = "Coluna lida pela Lambda de autenticação deve existir com o tipo do contrato")]
        [Trait("Integration", "Contrato")]
        [MemberData(nameof(ColunasDoContrato))]
        public async Task Coluna_Do_Contrato_Deve_Existir_Com_Tipo_Esperado(string tabela, string coluna, string tipoEsperado)
        {
            using var scope = integrationTestFactory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CadastroContext>();
            var connection = context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = ConsultaTipoColuna;
            command.Parameters.Add(CriarParametro(command, "tabela", tabela));
            command.Parameters.Add(CriarParametro(command, "coluna", coluna));

            var tipoEncontrado = await command.ExecuteScalarAsync() as string;

            Assert.Equal(tipoEsperado, tipoEncontrado);
        }

        private static DbParameter CriarParametro(DbCommand command, string nome, string valor)
        {
            var parametro = command.CreateParameter();
            parametro.ParameterName = nome;
            parametro.Value = valor;
            return parametro;
        }
    }
}
