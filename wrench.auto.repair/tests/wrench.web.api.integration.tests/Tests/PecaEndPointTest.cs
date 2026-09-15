using Bogus;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using wrench.auto.repair.autenticacao.domain.Entities;
using wrench.auto.repair.autenticacao.domain.Security;
using wrench.auto.repair.core.ValueObjects;
using wrench.web.api.integration.tests.Base;

namespace wrench.web.api.integration.tests.Tests
{
    [Collection("SharedContainer")]
    public class PecaEndPointTest
    {
        private readonly HttpClient _httpClient;

        public PecaEndPointTest(IntegrationTestFactory integrationTestFactory)
        {
            _httpClient = integrationTestFactory.CreateClient();

            using var scope = integrationTestFactory.Services.CreateScope();
            var jwtGenerator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();

            var email = new Email("admin.peca@teste.com");
            var perfil = new Perfil("Admin", "Administrador", true, DateTime.UtcNow);

            var usuario = new Usuario(email, perfil.Id, true, DateTime.UtcNow);
            typeof(Usuario).GetProperty("Perfil")?.SetValue(usuario, perfil, null);

            var token = jwtGenerator.GerarToken(usuario);
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);
        }

        private async Task<(Guid Id, string Nome)> CriarPecaAsync(double valor = 99.90, int quantidade = 50)
        {
            var nome = $"PECA-{Guid.NewGuid():N}";
            var faker = new Faker("pt_BR");
            var res = await _httpClient.PostAsJsonAsync("/api/v1/peca", new
            {
                nome,
                descricao = $"{faker.Commerce.ProductDescription()} integração",
                valor,
                quantidade,
                ativo = true
            });
            res.EnsureSuccessStatusCode();
            var id = await res.Content.ReadFromJsonAsync<Guid>();
            return (id, nome);
        }

        [Fact(DisplayName = "Listar Peças Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Listar_Pecas_Com_Sucesso()
        {
            var response = await _httpClient.GetAsync("/api/v1/peca?numeroPagina=1&tamanhoPagina=5");

            response.EnsureSuccessStatusCode();
        }

        [Fact(DisplayName = "Listar Peças Com Falha Quando Ordenação Inválida")]
        [Trait("Integration", "WebApi")]
        public async Task Listar_Pecas_Com_Falha_Ordenacao()
        {
            var response = await _httpClient.GetAsync("/api/v1/peca?ordenarPor=InvalidSort");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact(DisplayName = "Obter Peça Por Id Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Obter_Peca_Por_Id_Com_Sucesso()
        {
            var (id, _) = await CriarPecaAsync();

            var response = await _httpClient.GetAsync($"/api/v1/peca/{id}");

            response.EnsureSuccessStatusCode();
        }

        [Fact(DisplayName = "Obter Peça Por Id Com Falha Quando Não Existir")]
        [Trait("Integration", "WebApi")]
        public async Task Obter_Peca_Por_Id_Com_Falha()
        {
            var response = await _httpClient.GetAsync($"/api/v1/peca/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "Cadastrar Peça Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Cadastrar_Peca_Com_Sucesso()
        {
            var nome = $"NOVA-PECA-{Guid.NewGuid():N}";
            var response = await _httpClient.PostAsJsonAsync("/api/v1/peca", new
            {
                nome,
                descricao = "Cadastro integração peça",
                valor = 55.5,
                quantidade = 12,
                ativo = true
            });

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact(DisplayName = "Cadastrar Peça Com Falha Quando Nome Já Existir")]
        [Trait("Integration", "WebApi")]
        public async Task Cadastrar_Peca_Com_Falha_Nome_Duplicado()
        {
            var nome = $"DUP-PECA-{Guid.NewGuid():N}";

            var primeiro = await _httpClient.PostAsJsonAsync("/api/v1/peca", new
            {
                nome,
                descricao = "Primeira",
                valor = 10.0,
                quantidade = 1,
                ativo = true
            });
            primeiro.EnsureSuccessStatusCode();

            var response = await _httpClient.PostAsJsonAsync("/api/v1/peca", new
            {
                nome,
                descricao = "Segunda igual nome",
                valor = 20.0,
                quantidade = 2,
                ativo = true
            });

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact(DisplayName = "Atualizar Peça Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Atualizar_Peca_Com_Sucesso()
        {
            var (id, nomeInicial) = await CriarPecaAsync(40, 5);

            var response = await _httpClient.PutAsJsonAsync("/api/v1/peca", new
            {
                id,
                nome = nomeInicial,
                descricao = "Descrição atualizada integração",
                valor = 88.0,
                ativo = true
            });

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact(DisplayName = "Atualizar Peça Com Falha Quando Não Existir")]
        [Trait("Integration", "WebApi")]
        public async Task Atualizar_Peca_Com_Falha()
        {
            var response = await _httpClient.PutAsJsonAsync("/api/v1/peca", new
            {
                id = Guid.NewGuid(),
                nome = "X",
                descricao = "Y",
                valor = 1.0,
                ativo = true
            });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "Repor Estoque Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Repor_Estoque_Com_Sucesso()
        {
            var (id, _) = await CriarPecaAsync(15, 3);

            var response = await _httpClient.PutAsync(
                $"/api/v1/peca/{id}/repor?quantidade=7",
                new StringContent(string.Empty, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact(DisplayName = "Repor Estoque Com Falha Quando Quantidade Inválida")]
        [Trait("Integration", "WebApi")]
        public async Task Repor_Estoque_Com_Falha()
        {
            var (id, _) = await CriarPecaAsync();

            var response = await _httpClient.PutAsync(
                $"/api/v1/peca/{id}/repor?quantidade=0",
                new StringContent(string.Empty, Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact(DisplayName = "Baixar Estoque Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Baixar_Estoque_Com_Sucesso()
        {
            var (id, _) = await CriarPecaAsync(25, 20);

            var response = await _httpClient.PutAsync(
                $"/api/v1/peca/{id}/baixar?quantidade=2",
                new StringContent(string.Empty, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact(DisplayName = "Baixar Estoque Com Falha Quando Peça Não Existir")]
        [Trait("Integration", "WebApi")]
        public async Task Baixar_Estoque_Com_Falha()
        {
            var response = await _httpClient.PutAsync(
                $"/api/v1/peca/{Guid.NewGuid()}/baixar?quantidade=1",
                new StringContent(string.Empty, Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "Desativar Peça Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Desativar_Peca_Com_Sucesso()
        {
            var (id, _) = await CriarPecaAsync();

            var response = await _httpClient.PutAsync(
                $"/api/v1/peca/{id}/desativar",
                new StringContent(string.Empty, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact(DisplayName = "Desativar Peça Com Falha Quando Peça Não Existir")]
        [Trait("Integration", "WebApi")]
        public async Task Desativar_Peca_Com_Falha()
        {
            var response = await _httpClient.PutAsync(
                $"/api/v1/peca/{Guid.NewGuid()}/desativar",
                new StringContent(string.Empty, Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "Ativar Peça Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Ativar_Peca_Com_Sucesso()
        {
            var (id, _) = await CriarPecaAsync();
            var des = await _httpClient.PutAsync(
                $"/api/v1/peca/{id}/desativar",
                new StringContent(string.Empty, Encoding.UTF8, "application/json"));
            des.EnsureSuccessStatusCode();

            var response = await _httpClient.PutAsync(
                $"/api/v1/peca/{id}/ativar",
                new StringContent(string.Empty, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact(DisplayName = "Ativar Peça Com Falha Quando Peça Não Existir")]
        [Trait("Integration", "WebApi")]
        public async Task Ativar_Peca_Com_Falha()
        {
            var response = await _httpClient.PutAsync(
                $"/api/v1/peca/{Guid.NewGuid()}/ativar",
                new StringContent(string.Empty, Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
