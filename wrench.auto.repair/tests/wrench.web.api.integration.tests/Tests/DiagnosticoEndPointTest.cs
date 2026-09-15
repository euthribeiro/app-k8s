using Bogus;
using Bogus.Extensions.Brazil;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using wrench.auto.repair.autenticacao.domain.Entities;
using wrench.auto.repair.autenticacao.domain.Security;
using wrench.auto.repair.core.ValueObjects;
using wrench.auto.repair.ordem.servico.application.UseCases.DiagnosticoUseCase;
using wrench.auto.repair.ordem.servico.application.UseCases.OrdemServicoUseCase;
using wrench.web.api.integration.tests.Base;
using wrench.web.api.Models.Diagnostico;

namespace wrench.web.api.integration.tests.Tests
{
    [Collection("SharedContainer")]
    public class DiagnosticoEndPointTest
    {
        private static string PlacaAleatoria(string prefix)
        {
            var letras = "ABCDEFGHJKLMNPRSTUVXYZ";
            var c = letras[Random.Shared.Next(letras.Length)];
            return $"{prefix}-1{c}{Random.Shared.Next(10, 99)}";
        }

        private readonly HttpClient _httpClient;
        private readonly IntegrationTestFactory _integrationTestFactory;

        public DiagnosticoEndPointTest(IntegrationTestFactory integrationTestFactory)
        {
            _integrationTestFactory = integrationTestFactory;
            _httpClient = _integrationTestFactory.CreateClient();

            using var scope = _integrationTestFactory.Services.CreateScope();
            var jwtGenerator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();

            var email = new Email("teste@teste.com");
            var perfil = new Perfil("Admin", "Administrador", true, DateTime.UtcNow);

            var usuario = new Usuario(email, perfil.Id, true, DateTime.UtcNow);
            typeof(Usuario).GetProperty("Perfil")?.SetValue(usuario, perfil, null);

            var token = jwtGenerator.GerarToken(usuario);
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);
        }

        [Fact(DisplayName = "Realizar Diagnóstico Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Realizar_Diagnostico_Com_Sucesso()
        {
            using var scope = _integrationTestFactory.Services.CreateScope();
            var mediatoR = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();

            var enderecoDto = new wrench.auto.repair.cadastro.application.Commands.Dtos.EnderecoDto(
                "Rua Teste",
                "123",
                "Apto 1",
                "Bairro Teste",
                "01234-567",
                "São Paulo",
                "SP",
                "Brasil"
            );

            var clienteCommand = new wrench.auto.repair.cadastro.application.Commands.CadastrarClienteCommand(
                "92761932005",
                "Cliente Teste",
                "11999999999",
                "cliente@teste1.com",
                enderecoDto
            );

            var clienteResponse = await mediatoR.Send(clienteCommand);
            var clienteId = clienteResponse.Valor;

            var veiculoCommand = new wrench.auto.repair.cadastro.application.Commands.CadastrarVeiculoCommand(
                clienteId,
                "Marca Teste",
                "Modelo Teste",
                "Cor Teste",
                2020,
                2021,
                "ABC-1235",
                "Veiculo Teste",
                DateTime.UtcNow,
                10000
            );

            var veiculoResponse = await mediatoR.Send(veiculoCommand);
            var veiculoId = veiculoResponse.Valor;

            var ordemServicoCommand = new CriarOrdemServicoCommand(clienteId, veiculoId, "Problema no sistema de freios");

            var ordemServicoResponse = await mediatoR.Send(ordemServicoCommand);
            var ordemServicoId = ordemServicoResponse.Valor;

            var solicitaDiagnostico = new SolicitarDiagnosticoCommand(ordemServicoId);
            var diagnosticoResponse = await mediatoR.Send(solicitaDiagnostico);

            var request = new RealizarDiagnosticoRequest
            {
                OrdemServicoId = ordemServicoId,
                SolucaoProposta = "Substituição das pastilhas de freio",
                ValorEstimado = 500.00m,
                Pecas = []
            };

            var response = await _httpClient.PostAsJsonAsync("/api/v1/diagnostico", request);

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact(DisplayName = "Solicitar Diagnóstico Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Solicitar_Diagnostico_Com_Sucesso()
        {
            using var scope = _integrationTestFactory.Services.CreateScope();
            var mediatoR = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();
            var faker = new Faker("pt_BR");

            var enderecoDto = new wrench.auto.repair.cadastro.application.Commands.Dtos.EnderecoDto(
                "Rua Diagnóstico Put",
                "10",
                null,
                "Centro",
                "01310-100",
                "São Paulo",
                "SP",
                "Brasil");

            var documento = faker.Person.Cpf().Replace(".", string.Empty).Replace("-", string.Empty);

            var clienteResponse = await mediatoR.Send(
                new wrench.auto.repair.cadastro.application.Commands.CadastrarClienteCommand(
                    documento,
                    faker.Person.FullName,
                    "11987654321",
                    $"{Guid.NewGuid():N}@diag.put.integracao.com",
                    enderecoDto));
            Assert.True(clienteResponse.Sucesso);
            var clienteId = clienteResponse.Valor;

            var placaDiag = PlacaAleatoria("PDU");

            var veiculoResponse = await mediatoR.Send(
                new wrench.auto.repair.cadastro.application.Commands.CadastrarVeiculoCommand(
                    clienteId,
                    "Marca",
                    "Modelo",
                    "Prata",
                    2023,
                    2024,
                    placaDiag,
                    "Veículo diagnóstico PUT",
                    DateTime.UtcNow,
                    5000));
            Assert.True(veiculoResponse.Sucesso);
            var veiculoId = veiculoResponse.Valor;

            var ordemResponse = await mediatoR.Send(
                new CriarOrdemServicoCommand(clienteId, veiculoId, "Revisão geral"));
            Assert.True(ordemResponse.Sucesso);
            var ordemServicoId = ordemResponse.Valor;

            var request = new SolicitarDiagnosticoRequest { OrdemServicoId = ordemServicoId };

            var response = await _httpClient.PutAsJsonAsync("/api/v1/diagnostico", request);

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact(DisplayName = "Solicitar Diagnóstico Com Falha Quando Ordem Não Existir")]
        [Trait("Integration", "WebApi")]
        public async Task Solicitar_Diagnostico_Com_Falha_Quando_Nao_Existir()
        {
            var request = new SolicitarDiagnosticoRequest { OrdemServicoId = Guid.NewGuid() };

            var response = await _httpClient.PutAsJsonAsync("/api/v1/diagnostico", request);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "Registrar Diagnóstico Com Falha Quando Ordem Não Existir")]
        [Trait("Integration", "WebApi")]
        public async Task Registrar_Diagnostico_Com_Falha_Quando_Nao_Existir()
        {
            var request = new RealizarDiagnosticoRequest
            {
                OrdemServicoId = Guid.NewGuid(),
                SolucaoProposta = "Trocar filtros",
                ValorEstimado = 300m,
                Pecas = []
            };

            var response = await _httpClient.PostAsJsonAsync("/api/v1/diagnostico", request);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
