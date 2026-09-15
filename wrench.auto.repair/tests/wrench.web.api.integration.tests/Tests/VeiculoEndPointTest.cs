using Bogus;
using Bogus.Extensions.Brazil;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using wrench.auto.repair.autenticacao.domain.Entities;
using wrench.auto.repair.autenticacao.domain.Security;
using wrench.auto.repair.core.ValueObjects;
using wrench.web.api.integration.tests.Base;

namespace wrench.web.api.integration.tests.Tests
{
    [Collection("SharedContainer")]
    public class VeiculoEndPointTest
    {
        private readonly HttpClient _httpClient;

        public VeiculoEndPointTest(IntegrationTestFactory integrationTestFactory)
        {
            _httpClient = integrationTestFactory.CreateClient();

            using var scope = integrationTestFactory.Services.CreateScope();
            var jwtGenerator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();

            var email = new Email("admin.veiculo@teste.com");
            var perfil = new Perfil("Admin", "Administrador", true, DateTime.UtcNow);

            var usuario = new Usuario(email, perfil.Id, true, DateTime.UtcNow);
            typeof(Usuario).GetProperty("Perfil")?.SetValue(usuario, perfil, null);

            var token = jwtGenerator.GerarToken(usuario);
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);
        }

        /// Mercosul: três letras + 1 dígito + letra+número + 2 dígitos (ex.: TST-1B34).
        private static string PlacaAleatoria(string prefix)
        {
            var letras = "ABCDEFGHJKLMNPRSTUVXYZ";
            var c = letras[Random.Shared.Next(letras.Length)];
            return $"{prefix}-1{c}{Random.Shared.Next(10, 99)}";
        }

        private async Task<Guid> CadastrarClienteAsync()
        {
            var faker = new Faker("pt_BR");
            var documento = faker.Person.Cpf().Replace(".", string.Empty).Replace("-", string.Empty);
            var res = await _httpClient.PostAsJsonAsync("/api/v1/cliente", new
            {
                documento,
                nome = faker.Person.FullName,
                telefone = "11987654321",
                email = $"{Guid.NewGuid():N}@integracao.veiculo.com",
                endereco = new
                {
                    logradouro = "Rua Veículo",
                    numero = "1",
                    complemento = "A",
                    bairro = "Centro",
                    cep = "01310-100",
                    cidade = "São Paulo",
                    unidadeFederativa = "SP",
                    pais = "Brasil"
                }
            });
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<Guid>();
        }

        [Fact(DisplayName = "Listar Veículos Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Listar_Veiculos_Com_Sucesso()
        {
            var response = await _httpClient.GetAsync("/api/v1/veiculo?numeroPagina=1&tamanhoPagina=5");

            response.EnsureSuccessStatusCode();
        }

        [Fact(DisplayName = "Listar Veículos Com Falha Quando Ordenação Inválida")]
        [Trait("Integration", "WebApi")]
        public async Task Listar_Veiculos_Com_Falha_Quando_Ordenacao_Invalida()
        {
            var response = await _httpClient.GetAsync("/api/v1/veiculo?ordenarPor=CampoInexistente");

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact(DisplayName = "Cadastrar Veículo Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Cadastrar_Veiculo_Com_Sucesso()
        {
            var clienteId = await CadastrarClienteAsync();
            var placa = PlacaAleatoria("TST");

            var response = await _httpClient.PostAsJsonAsync("/api/v1/veiculo", new
            {
                clienteId,
                marca = "Volkswagen",
                modelo = "Gol",
                cor = "Branco",
                anoFabricacao = 2023,
                anoModelo = 2024,
                placaDoVeiculo = placa,
                descricao = "Integração cadastro veículo",
                ultimaRevisao = (DateTime?)DateTime.UtcNow,
                quilometragemAtual = 15000
            });

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact(DisplayName = "Cadastrar Veículo Com Falha Quando Cliente Não Existir")]
        [Trait("Integration", "WebApi")]
        public async Task Cadastrar_Veiculo_Com_Falha_Cliente_Invalido()
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1/veiculo", new
            {
                clienteId = Guid.NewGuid(),
                marca = "Fiat",
                modelo = "Uno",
                cor = "Azul",
                anoFabricacao = 2022,
                anoModelo = 2023,
                placaDoVeiculo = PlacaAleatoria("INV"),
                descricao = "Cliente inexistente",
                ultimaRevisao = (DateTime?)DateTime.UtcNow,
                quilometragemAtual = 9000
            });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "Obter Veículo Por Id Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Obter_Veiculo_Por_Id_Com_Sucesso()
        {
            var clienteId = await CadastrarClienteAsync();
            var placa = PlacaAleatoria("IDT");

            var post = await _httpClient.PostAsJsonAsync("/api/v1/veiculo", new
            {
                clienteId,
                marca = "Honda",
                modelo = "Civic",
                cor = "Preto",
                anoFabricacao = 2021,
                anoModelo = 2022,
                placaDoVeiculo = placa,
                descricao = "Consulta por id",
                ultimaRevisao = (DateTime?)null,
                quilometragemAtual = 40000
            });
            post.EnsureSuccessStatusCode();
            var veiculoId = await post.Content.ReadFromJsonAsync<Guid>();

            var response = await _httpClient.GetAsync($"/api/v1/veiculo/{veiculoId}");

            response.EnsureSuccessStatusCode();
        }

        [Fact(DisplayName = "Obter Veículo Por Id Com Falha Quando Não Existir")]
        [Trait("Integration", "WebApi")]
        public async Task Obter_Veiculo_Por_Id_Com_Falha()
        {
            var response = await _httpClient.GetAsync($"/api/v1/veiculo/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "Obter Veículo Por Placa Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Obter_Veiculo_Por_Placa_Com_Sucesso()
        {
            var clienteId = await CadastrarClienteAsync();
            var placa = PlacaAleatoria("PLC");

            var post = await _httpClient.PostAsJsonAsync("/api/v1/veiculo", new
            {
                clienteId,
                marca = "Toyota",
                modelo = "Corolla",
                cor = "Prata",
                anoFabricacao = 2020,
                anoModelo = 2021,
                placaDoVeiculo = placa,
                descricao = "Consulta placa",
                ultimaRevisao = (DateTime?)null,
                quilometragemAtual = 60000
            });
            post.EnsureSuccessStatusCode();

            var placaSemHifen = placa.Replace("-", string.Empty);

            var response = await _httpClient.GetAsync($"/api/v1/veiculo/{placaSemHifen}");

            response.EnsureSuccessStatusCode();
        }

        [Fact(DisplayName = "Obter Veículo Por Placa Com Falha Quando Não Existir")]
        [Trait("Integration", "WebApi")]
        public async Task Obter_Veiculo_Por_Placa_Com_Falha()
        {
            var response = await _httpClient.GetAsync("/api/v1/veiculo/ZZZ9Z99");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "Atualizar Veículo Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Atualizar_Veiculo_Com_Sucesso()
        {
            var clienteId = await CadastrarClienteAsync();
            var placa = PlacaAleatoria("UPD");

            var post = await _httpClient.PostAsJsonAsync("/api/v1/veiculo", new
            {
                clienteId,
                marca = "Chevrolet",
                modelo = "Onix",
                cor = "Vermelho",
                anoFabricacao = 2019,
                anoModelo = 2020,
                placaDoVeiculo = placa,
                descricao = "Antes update",
                ultimaRevisao = (DateTime?)null,
                quilometragemAtual = 30000
            });
            post.EnsureSuccessStatusCode();
            var veiculoId = await post.Content.ReadFromJsonAsync<Guid>();

            var response = await _httpClient.PutAsJsonAsync("/api/v1/veiculo", new
            {
                veiculoId,
                clienteId,
                marca = "Chevrolet",
                modelo = "Onix Plus",
                cor = "Cinza",
                anoFabricacao = 2020,
                anoModelo = 2021,
                placaDoVeiculo = placa,
                descricao = "Depois update",
                ultimaRevisao = (DateTime?)DateTime.UtcNow,
                quilometragemAtual = 32000
            });

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact(DisplayName = "Atualizar Veículo Com Falha Quando Não Existir")]
        [Trait("Integration", "WebApi")]
        public async Task Atualizar_Veiculo_Com_Falha()
        {
            var clienteId = await CadastrarClienteAsync();

            var response = await _httpClient.PutAsJsonAsync("/api/v1/veiculo", new
            {
                veiculoId = Guid.NewGuid(),
                clienteId,
                marca = "X",
                modelo = "Y",
                cor = "Z",
                anoFabricacao = 2024,
                anoModelo = 2025,
                placaDoVeiculo = PlacaAleatoria("XXX"),
                descricao = "Inexistente",
                ultimaRevisao = (DateTime?)null,
                quilometragemAtual = 0
            });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
