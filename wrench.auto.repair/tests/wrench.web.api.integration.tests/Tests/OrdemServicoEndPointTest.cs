using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using wrench.auto.repair.autenticacao.domain.Entities;
using wrench.auto.repair.autenticacao.domain.Security;
using wrench.auto.repair.core.ValueObjects;
using wrench.auto.repair.ordem.servico.application.UseCases.DiagnosticoUseCase;
using wrench.auto.repair.ordem.servico.application.UseCases.OrdemServicoUseCase;
using wrench.web.api.integration.tests.Base;
using wrench.web.api.Models.OrdemServico;

namespace wrench.web.api.integration.tests.Tests
{
    [Collection("SharedContainer")]
    public class OrdemServicoEndPointTest
    {
        private readonly HttpClient _httpClient;
        private readonly IntegrationTestFactory _integrationTestFactory;

        public OrdemServicoEndPointTest(IntegrationTestFactory integrationTestFactory)
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

        [Fact(DisplayName = "Criar Ordem de Serviço Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Criar_Ordem_Servico_Com_Sucesso()
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
                "07940660039",
                "Cliente Teste",
                "11999999999",
                "cliente@teste.com",
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
                "ABC-1234",
                "Veiculo Teste",
                DateTime.UtcNow,
                10000
            );

            var veiculoResponse = await mediatoR.Send(veiculoCommand);
            var veiculoId = veiculoResponse.Valor;

            var createRequest = new CriarOrdemServicoRequest
            {
                ClienteId = clienteId,
                VeiculoId = veiculoId,
                Descricao = "Teste de criação de ordem de serviço"
            };

            var response = await _httpClient.PostAsJsonAsync("/api/v1/ordem-servico", createRequest);

            response.EnsureSuccessStatusCode();
        }

        [Fact(DisplayName = "Finalizar Ordem de Serviço Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Finalizar_Ordem_Servico_Com_Sucesso()
        {
            using var scope = _integrationTestFactory.Services.CreateScope();
            var mediatoR = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();

            var enderecoDto = new wrench.auto.repair.cadastro.application.Commands.Dtos.EnderecoDto(
                "Rua Teste Finalizar",
                "123",
                "Apto 1",
                "Bairro Teste",
                "01234-567",
                "São Paulo",
                "SP",
                "Brasil"
            );

            var clienteCommand = new wrench.auto.repair.cadastro.application.Commands.CadastrarClienteCommand(
                "01905075006",
                "Cliente Teste Finalizar",
                "11999999999",
                "ordem.service.finalizar@teste.com",
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
                "XYZ-1234",
                "Veiculo Teste Finalizar",
                DateTime.UtcNow,
                10000
            );

            var veiculoResponse = await mediatoR.Send(veiculoCommand);
            var veiculoId = veiculoResponse.Valor;

            var criarCommand = new CriarOrdemServicoCommand(clienteId, veiculoId, "Teste de finalização de ordem de serviço");
            var ordemServicoResponse = await mediatoR.Send(criarCommand);
            var ordemServicoId = ordemServicoResponse.Valor;

            var solicitarDiagnosticoCommand = new SolicitarDiagnosticoCommand(ordemServicoId);
            await mediatoR.Send(solicitarDiagnosticoCommand);

            var realizarDiagnosticoCommand = new RealizarDiagnosticoCommand(ordemServicoId, 199.99m, "Solução proposta", []);
            await mediatoR.Send(realizarDiagnosticoCommand);

            var aprovaOrcamentoCommand = new wrench.auto.repair.ordem.servico.application.UseCases.OrcamentoUseCase.AprovaOrcamentoCommand(ordemServicoId);
            await mediatoR.Send(aprovaOrcamentoCommand);

            var response = await _httpClient.PutAsync($"/api/v1/ordem-servico/{ordemServicoId}/finalizar", null);

            response.EnsureSuccessStatusCode();
        }

        [Fact(DisplayName = "Obter Ordem de Serviço Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Obter_Ordem_Servico_Com_Sucesso()
        {
            using var scope = _integrationTestFactory.Services.CreateScope();
            var mediatoR = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();

            var enderecoDto = new wrench.auto.repair.cadastro.application.Commands.Dtos.EnderecoDto(
                "Rua Teste Consulta",
                "123",
                "Apto 1",
                "Bairro Teste",
                "01234-567",
                "São Paulo",
                "SP",
                "Brasil"
            );

            var clienteCommand = new wrench.auto.repair.cadastro.application.Commands.CadastrarClienteCommand(
                "08173460078",
                "Cliente Teste Consulta",
                "11999999999",
                "cliente.consulta@teste.com",
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
                "QWE-1234",
                "Veiculo Teste Consulta",
                DateTime.UtcNow,
                10000
            );

            var veiculoResponse = await mediatoR.Send(veiculoCommand);
            var veiculoId = veiculoResponse.Valor;

            var criarCommand = new CriarOrdemServicoCommand(clienteId, veiculoId, "Teste de consulta de ordem de serviço");
            var ordemServicoResponse = await mediatoR.Send(criarCommand);
            var ordemServicoId = ordemServicoResponse.Valor;

            var response = await _httpClient.GetAsync($"/api/v1/ordem-servico?id={ordemServicoId}");

            response.EnsureSuccessStatusCode();
        }

        [Fact(DisplayName = "Obter Ordem de Serviço Com Falha Quando Não Existir")]
        [Trait("Integration", "WebApi")]
        public async Task Obter_Ordem_Servico_Com_Falha_Quando_Nao_Existir()
        {
            var ordemServicoId = Guid.NewGuid();

            var response = await _httpClient.GetAsync($"/api/v1/ordem-servico/{ordemServicoId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "Criar Ordem de Serviço Com Falha Quando Cliente ou Veículo Não Existir")]
        [Trait("Integration", "WebApi")]
        public async Task Criar_Ordem_Servico_Com_Falha_Quando_Cliente_Veiculo_Invalidos()
        {
            var request = new CriarOrdemServicoRequest
            {
                ClienteId = Guid.NewGuid(),
                VeiculoId = Guid.NewGuid(),
                Descricao = "Ordem inválida"
            };

            var response = await _httpClient.PostAsJsonAsync("/api/v1/ordem-servico", request);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "Finalizar Ordem de Serviço Com Falha Quando Ordem Não Existir")]
        [Trait("Integration", "WebApi")]
        public async Task Finalizar_Ordem_Servico_Com_Falha_Quando_Nao_Existir()
        {
            var id = Guid.NewGuid();

            var response = await _httpClient.PutAsync($"/api/v1/ordem-servico/{id}/finalizar", null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
