using Bogus;
using Bogus.Extensions.Brazil;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using wrench.auto.repair.autenticacao.domain.Entities;
using wrench.auto.repair.autenticacao.domain.Security;
using wrench.auto.repair.core.ValueObjects;
using wrench.auto.repair.estoque.application.Commands;
using wrench.auto.repair.ordem.servico.application.UseCases.DiagnosticoUseCase;
using wrench.auto.repair.ordem.servico.application.UseCases.OrdemServicoUseCase;
using wrench.web.api.integration.tests.Base;

namespace wrench.web.api.integration.tests.Tests
{
    [Collection("SharedContainer")]
    public class OrcamentoEndPointTest
    {
        private static string PlacaAleatoria(string prefix)
        {
            var letras = "ABCDEFGHJKLMNPRSTUVXYZ";
            var c = letras[Random.Shared.Next(letras.Length)];
            return $"{prefix}-1{c}{Random.Shared.Next(10, 99)}";
        }

        private readonly HttpClient _httpClient;
        private readonly IntegrationTestFactory _integrationTestFactory;

        public OrcamentoEndPointTest(IntegrationTestFactory integrationTestFactory)
        {
            _integrationTestFactory = integrationTestFactory;
            _httpClient = _integrationTestFactory.CreateClient();

            using var scope = _integrationTestFactory.Services.CreateScope();
            var jwtGenerator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();

            var email = new Email("brunocbarreto2012@gmail.com");
            var perfil = new Perfil("Cliente", "Cliente do sistema", true, DateTime.UtcNow);

            var usuario = new Usuario(email, perfil.Id, true, DateTime.UtcNow);
            typeof(Usuario).GetProperty("Perfil")?.SetValue(usuario, perfil, null);

            var token = jwtGenerator.GerarToken(usuario);
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);
        }

        [Fact(DisplayName = "Aprovar orçamento com sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Aprovar_Orcamento_Com_Sucesso()
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
                "83318154083",
                "Cliente Teste",
                "11999999998",
                "brunocbarreto2012@gmail.com",
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
                "ABC-1237",
                "Veiculo Teste",
                DateTime.UtcNow,
                10000
            );

            var veiculoResponse = await mediatoR.Send(veiculoCommand);
            var veiculoId = veiculoResponse.Valor;

            var ordemServicoCommand = new CriarOrdemServicoCommand(clienteId, veiculoId, "Problema no sistema de freios");

            var ordemServicoResponse = await mediatoR.Send(ordemServicoCommand);
            var ordemServicoId = ordemServicoResponse.Valor;

            var nomePecaUnico = $"PASTILHA FREIO ORC INT {Guid.NewGuid():N}";
            var cadastrarPeca = new CadastrarPecaCommand(
                nomePecaUnico,
                "Pastilha para sistema de freios — teste integração orçamento",
                149.90,
                20,
                true);
            var pecaResponse = await mediatoR.Send(cadastrarPeca);
            var pecaId = pecaResponse.Valor;

            var solicitaDiagnostico = new SolicitarDiagnosticoCommand(ordemServicoId);
            var diagnosticoResponse = await mediatoR.Send(solicitaDiagnostico);

            var pecasIds = new HashSet<Guid> { pecaId };
            var registrarDiagnostico = new RealizarDiagnosticoCommand(ordemServicoId, 500.00m, "Problema identificado", pecasIds);
            var registrarDiagnosticoResponse = await mediatoR.Send(registrarDiagnostico);

            var response = await _httpClient.PutAsync(
                $"/api/v1/orcamento/{ordemServicoId}/aprovar",
                new StringContent(string.Empty, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact(DisplayName = "Recusar orçamento com sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Recusar_Orcamento_Com_Sucesso()
        {
            using var scope = _integrationTestFactory.Services.CreateScope();
            var mediatoR = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();
            var faker = new Faker("pt_BR");

            var enderecoDto = new wrench.auto.repair.cadastro.application.Commands.Dtos.EnderecoDto(
                "Rua Orçamento Recusa",
                "55",
                null,
                "Centro",
                "01234-567",
                "São Paulo",
                "SP",
                "Brasil");

            var documento = faker.Person.Cpf().Replace(".", string.Empty).Replace("-", string.Empty);

            var clienteResponse = await mediatoR.Send(
                new wrench.auto.repair.cadastro.application.Commands.CadastrarClienteCommand(
                    documento,
                    faker.Person.FullName,
                    "11987654321",
                    $"{Guid.NewGuid():N}@recusa.orc.integracao.com",
                    enderecoDto));
            Assert.True(clienteResponse.Sucesso);
            var clienteId = clienteResponse.Valor;

            var placa = PlacaAleatoria("REC");

            var veiculoResponse = await mediatoR.Send(
                new wrench.auto.repair.cadastro.application.Commands.CadastrarVeiculoCommand(
                    clienteId,
                    "Marca R",
                    "Modelo R",
                    "Azul",
                    2024,
                    2025,
                    placa,
                    "Veículo recusa orçamento",
                    DateTime.UtcNow,
                    12000));
            Assert.True(veiculoResponse.Sucesso);
            var veiculoId = veiculoResponse.Valor;

            var ordemResponse = await mediatoR.Send(
                new CriarOrdemServicoCommand(clienteId, veiculoId, "Painel aceso"));
            Assert.True(ordemResponse.Sucesso);
            var ordemServicoId = ordemResponse.Valor;

            var solicitado = await mediatoR.Send(new SolicitarDiagnosticoCommand(ordemServicoId));
            Assert.True(solicitado.Sucesso);

            var nomePeca = $"OLEO RECUSA {Guid.NewGuid():N}";
            var pecaResponse = await mediatoR.Send(new CadastrarPecaCommand(
                nomePeca,
                "Oleo para teste integração recusa",
                89.9,
                10,
                true));
            Assert.True(pecaResponse.Sucesso);
            var pecaId = pecaResponse.Valor;

            var realizado = await mediatoR.Send(new RealizarDiagnosticoCommand(
                ordemServicoId,
                950m,
                "Substituição componente sensível",
                new HashSet<Guid> { pecaId }));
            Assert.True(realizado.Sucesso);

            var response = await _httpClient.PutAsJsonAsync(
                $"/api/v1/orcamento/{ordemServicoId}/recusar",
                new { motivoRecusa = "Orçamento fora do previsto pelo cliente integração." });

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact(DisplayName = "Aprovar orçamento com falha quando ordem não existir")]
        [Trait("Integration", "WebApi")]
        public async Task Aprovar_Orcamento_Com_Falha_Quando_Nao_Existir()
        {
            var id = Guid.NewGuid();
            var response = await _httpClient.PutAsync(
                $"/api/v1/orcamento/{id}/aprovar",
                new StringContent(string.Empty, Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "Recusar orçamento com falha quando ordem não existir")]
        [Trait("Integration", "WebApi")]
        public async Task Recusar_Orcamento_Com_Falha_Quando_Nao_Existir()
        {
            var id = Guid.NewGuid();
            var response = await _httpClient.PutAsJsonAsync(
                $"/api/v1/orcamento/{id}/recusar",
                new { motivoRecusa = "Motivo válido obrigatório." });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
