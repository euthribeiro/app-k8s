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
    public class ClienteEndPointTest
    {
        private readonly HttpClient _httpClient;

        public ClienteEndPointTest(IntegrationTestFactory integrationTestFactory)
        {
            _httpClient = integrationTestFactory.CreateClient();

            using var scope = integrationTestFactory.Services.CreateScope();
            var jwtGenerator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();

            var email = new Email("admin.cliente@teste.com");
            var perfil = new Perfil("Admin", "Administrador", true, DateTime.UtcNow);

            var usuario = new Usuario(email, perfil.Id, true, DateTime.UtcNow);
            typeof(Usuario).GetProperty("Perfil")?.SetValue(usuario, perfil, null);

            var token = jwtGenerator.GerarToken(usuario);
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);
        }

        private static object NovoEndereco(string logradouro) => new
        {
            logradouro,
            numero = "10",
            complemento = "Sala 1",
            bairro = "Centro",
            cep = "01310-100",
            cidade = "São Paulo",
            unidadeFederativa = "SP",
            pais = "Brasil"
        };

        private static Faker NovoFaker() => new("pt_BR");

        [Fact(DisplayName = "Listar Clientes Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Listar_Clientes_Com_Sucesso()
        {
            var response = await _httpClient.GetAsync("/api/v1/cliente?numeroPagina=1&tamanhoPagina=5");

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact(DisplayName = "Listar Clientes Com Falha Quando Ordenação Inválida")]
        [Trait("Integration", "WebApi")]
        public async Task Listar_Clientes_Com_Falha_Quando_Ordenacao_Invalida()
        {
            var response = await _httpClient.GetAsync("/api/v1/cliente?ordenarPor=NaoExisteCampoInvalido");

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact(DisplayName = "Obter Cliente Por Id Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Obter_Cliente_Por_Id_Com_Sucesso()
        {
            var faker = NovoFaker();
            var documento = faker.Person.Cpf().Replace(".", string.Empty).Replace("-", string.Empty);

            var postRes = await _httpClient.PostAsJsonAsync("/api/v1/cliente", new
            {
                documento,
                nome = "Cliente Integração Id",
                telefone = "11987654321",
                email = $"{Guid.NewGuid():N}@integracao.cliente.com",
                endereco = NovoEndereco("Rua Obter Por Id")
            });
            postRes.EnsureSuccessStatusCode();
            var clienteId = await postRes.Content.ReadFromJsonAsync<Guid>();

            var response = await _httpClient.GetAsync($"/api/v1/cliente/{clienteId}");

            response.EnsureSuccessStatusCode();
        }

        [Fact(DisplayName = "Obter Cliente Por Id Com Falha Quando Não Existir")]
        [Trait("Integration", "WebApi")]
        public async Task Obter_Cliente_Por_Id_Com_Falha_Quando_Nao_Existir()
        {
            var response = await _httpClient.GetAsync($"/api/v1/cliente/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "Obter Cliente Por Documento Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Obter_Cliente_Por_Documento_Com_Sucesso()
        {
            var faker = NovoFaker();
            var documento = faker.Person.Cpf().Replace(".", string.Empty).Replace("-", string.Empty);

            var postCliente = await _httpClient.PostAsJsonAsync("/api/v1/cliente", new
            {
                documento,
                nome = "Cliente Doc Integração",
                telefone = "11976543210",
                email = $"{Guid.NewGuid():N}@integracao.doc.com",
                endereco = NovoEndereco("Rua Por Documento")
            });
            postCliente.EnsureSuccessStatusCode();

            var response = await _httpClient.GetAsync($"/api/v1/cliente/{documento}");

            response.EnsureSuccessStatusCode();
        }

        [Fact(DisplayName = "Obter Cliente Por Documento Com Falha Quando Não Existir")]
        [Trait("Integration", "WebApi")]
        public async Task Obter_Cliente_Por_Documento_Com_Falha()
        {
            var response = await _httpClient.GetAsync("/api/v1/cliente/00000000000");

            Assert.False(response.IsSuccessStatusCode);
        }

        [Fact(DisplayName = "Cadastrar Cliente Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Cadastrar_Cliente_Com_Sucesso()
        {
            var faker = NovoFaker();
            var documento = faker.Person.Cpf().Replace(".", string.Empty).Replace("-", string.Empty);

            var response = await _httpClient.PostAsJsonAsync("/api/v1/cliente", new
            {
                documento,
                nome = "Cliente Novo Cadastro",
                telefone = "11965432109",
                email = $"{Guid.NewGuid():N}@integracao.cadastro.com",
                endereco = NovoEndereco("Rua Cadastro")
            });

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact(DisplayName = "Cadastrar Cliente Com Falha Quando Documento Já Existir")]
        [Trait("Integration", "WebApi")]
        public async Task Cadastrar_Cliente_Com_Falha_Documento_Duplicado()
        {
            var faker = NovoFaker();
            var documento = faker.Person.Cpf().Replace(".", string.Empty).Replace("-", string.Empty);
            var endereco = NovoEndereco("Rua Duplicado");

            var primeiro = await _httpClient.PostAsJsonAsync("/api/v1/cliente", new
            {
                documento,
                nome = "Primeiro Cadastro Dup",
                telefone = "11954321098",
                email = $"{Guid.NewGuid():N}@dup1.integracao.com",
                endereco
            });
            primeiro.EnsureSuccessStatusCode();

            var response = await _httpClient.PostAsJsonAsync("/api/v1/cliente", new
            {
                documento,
                nome = "Segundo Mesmo Doc",
                telefone = "11943210987",
                email = $"{Guid.NewGuid():N}@dup2.integracao.com",
                endereco
            });

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact(DisplayName = "Atualizar Cliente Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Atualizar_Cliente_Com_Sucesso()
        {
            var faker = NovoFaker();
            var documento = faker.Person.Cpf().Replace(".", string.Empty).Replace("-", string.Empty);

            var postRes = await _httpClient.PostAsJsonAsync("/api/v1/cliente", new
            {
                documento,
                nome = "Cliente Antes Update",
                telefone = "11932109876",
                email = $"{Guid.NewGuid():N}@antes.update.int.com",
                endereco = NovoEndereco("Rua Antes Update")
            });
            postRes.EnsureSuccessStatusCode();
            var clienteId = await postRes.Content.ReadFromJsonAsync<Guid>();

            var response = await _httpClient.PutAsJsonAsync("/api/v1/cliente", new
            {
                clienteId,
                nome = "Cliente Depois Update",
                telefone = "11921098765",
                email = $"{Guid.NewGuid():N}@depois.update.int.com",
                endereco = NovoEndereco("Rua Depois Update")
            });

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact(DisplayName = "Atualizar Cliente Com Falha Quando Não Existir")]
        [Trait("Integration", "WebApi")]
        public async Task Atualizar_Cliente_Com_Falha_Quando_Nao_Existir()
        {
            var putBody = new
            {
                clienteId = Guid.NewGuid(),
                nome = "Fulano Inexistente",
                telefone = "11987654321",
                email = "inexistente.pessoa@teste.com",
                endereco = NovoEndereco("Rua Inexistente")
            };

            var response = await _httpClient.PutAsJsonAsync("/api/v1/cliente", putBody);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
