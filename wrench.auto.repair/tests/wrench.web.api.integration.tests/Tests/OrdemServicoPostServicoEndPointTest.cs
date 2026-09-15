using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using wrench.auto.repair.autenticacao.domain.Entities;
using wrench.auto.repair.autenticacao.domain.Security;
using wrench.auto.repair.core.ValueObjects;
using wrench.web.api.integration.tests.Base;
using wrench.web.api.Models.OrdemServico;

namespace wrench.web.api.integration.tests.Tests;

[Collection("SharedContainer")]
public class OrdemServicoPostServicoEndPointTest
{
    private readonly HttpClient _httpClient;
    private readonly IntegrationTestFactory _integrationTestFactory;

    public OrdemServicoPostServicoEndPointTest(IntegrationTestFactory integrationTestFactory)
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

    [Fact(DisplayName = "Criar Ordem de Serviço com Cliente e Veículo com Sucesso")]
    [Trait("Integration", "WebApi")]
    public async Task PostServico_DeveCriarOrdemServico_ComSucesso()
    {
        var request = new NovaOrdemServicoRequest
        {
            Descricao = "Troca de óleo e filtros",
            Cliente = new wrench.auto.repair.cadastro.application.Commands.ViewModels.CadastrarClienteViewModel
            {
                Documento = "52998224725",
                Nome = "Cliente Post Servico",
                Telefone = "11988887777",
                Email = "cliente.postservico@teste.com",
                Endereco = new wrench.auto.repair.cadastro.application.Commands.ViewModels.CriarEditarEnderecoViewModel
                {
                    Logradouro = "Avenida Teste",
                    Numero = "1000",
                    Complemento = "Sala 10",
                    Bairro = "Centro",
                    Cep = "01010-010",
                    Cidade = "São Paulo",
                    UnidadeFederativa = "SP",
                    Pais = "Brasil"
                }
            },
            Veiculo = new wrench.auto.repair.cadastro.application.Commands.ViewModels.CadastrarVeiculoViewModel
            {
                Marca = "Honda",
                Modelo = "Civic",
                Cor = "Prata",
                AnoFabricacao = 2021,
                AnoModelo = 2022,
                PlacaDoVeiculo = "DEF-5678",
                Descricao = "Veículo cadastrado no PostServico",
                UltimaRevisao = DateTime.UtcNow,
                QuilometragemAtual = 15000
            }
        };

        var response = await _httpClient.PostAsJsonAsync("/api/v1/ordem-servico/servico", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
