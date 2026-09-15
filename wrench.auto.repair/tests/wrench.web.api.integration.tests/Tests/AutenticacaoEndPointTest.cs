using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using wrench.auto.repair.autenticacao.application.Commands;
using wrench.web.api.integration.tests.Base;

namespace wrench.web.api.integration.tests.Tests
{
    [Collection("SharedContainer")]
    public class AutenticacaoEndPointTest
    {
        private readonly IntegrationTestFactory _factory;

        public AutenticacaoEndPointTest(IntegrationTestFactory integrationTestFactory)
        {
            _factory = integrationTestFactory;
        }

        private static readonly Guid PerfilFuncionario =
            Guid.Parse("5665d39c-4907-40a9-b648-e9cbb041afed");

        private const string SenhaValida = "SenhaSeguraUm2#Tipo";

        [Fact(DisplayName = "Autenticar Usuário Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Autenticar_Usuario_Com_Sucesso()
        {
            var email = $"{Guid.NewGuid():N}@auth.integracao.com";

            using (var scope = _factory.Services.CreateScope())
            {
                var mediator = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();
                await mediator.Send(new CriarUsuarioCommand(email, SenhaValida, PerfilFuncionario, true));
            }

            var http = _factory.CreateClient();
            var response = await http.PostAsJsonAsync("/api/v1/autenticacao", new
            {
                email,
                senha = SenhaValida
            });

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact(DisplayName = "Autenticar Usuário Com Falha Quando Senha Inválida")]
        [Trait("Integration", "WebApi")]
        public async Task Autenticar_Usuario_Com_Falha_Senha_Invalida()
        {
            var email = $"{Guid.NewGuid():N}@auth.falha.com";

            using (var scope = _factory.Services.CreateScope())
            {
                var mediator = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();
                await mediator.Send(new CriarUsuarioCommand(email, SenhaValida, PerfilFuncionario, true));
            }

            var http = _factory.CreateClient();
            var response = await http.PostAsJsonAsync("/api/v1/autenticacao", new
            {
                email,
                senha = "SenhaErradaQualquer123#"
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
