using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using wrench.auto.repair.autenticacao.application.Commands;
using wrench.auto.repair.autenticacao.domain.Entities;
using wrench.auto.repair.autenticacao.domain.Security;
using wrench.auto.repair.core.ValueObjects;
using wrench.web.api.integration.tests.Base;

namespace wrench.web.api.integration.tests.Tests
{
    [Collection("SharedContainer")]
    public class UsuarioPerfilEndPointTest
    {
        private readonly HttpClient _httpAdmin;
        private readonly HttpClient _httpCliente;
        private readonly IntegrationTestFactory _factory;

        private static readonly Guid PerfilFuncionario = Guid.Parse("5665d39c-4907-40a9-b648-e9cbb041afed");

        private const string SenhaPadrao = "SenhaSeguraDois3#Tipo";

        public UsuarioPerfilEndPointTest(IntegrationTestFactory integrationTestFactory)
        {
            _factory = integrationTestFactory;
            _httpAdmin = integrationTestFactory.CreateClient();
            _httpCliente = integrationTestFactory.CreateClient();

            using var scope = integrationTestFactory.Services.CreateScope();
            var jwtGenerator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();

            var adminEmail = new Email("jwt.admin.usuario@teste.com");
            var adminPerfilEnt = new Perfil("Admin", "Administrador", true, DateTime.UtcNow);
            var adminUsuario = new Usuario(adminEmail, adminPerfilEnt.Id, true, DateTime.UtcNow);
            typeof(Usuario).GetProperty("Perfil")?.SetValue(adminUsuario, adminPerfilEnt, null);
            _httpAdmin.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtGenerator.GerarToken(adminUsuario).Token);

            var clienteEmail = new Email("jwt.cliente.perfil@teste.com");
            var clientePerfilEnt = new Perfil("Cliente", "Cliente", true, DateTime.UtcNow);
            var clienteUsuario = new Usuario(clienteEmail, clientePerfilEnt.Id, true, DateTime.UtcNow);
            typeof(Usuario).GetProperty("Perfil")?.SetValue(clienteUsuario, clientePerfilEnt, null);
            _httpCliente.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtGenerator.GerarToken(clienteUsuario).Token);
        }

        [Fact(DisplayName = "Listar Perfis Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Listar_Perfis_Com_Sucesso()
        {
            var response = await _httpAdmin.GetAsync("/api/v1/perfil");

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact(DisplayName = "Listar Perfis Com Falha Quando Perfil Não Autorizado")]
        [Trait("Integration", "WebApi")]
        public async Task Listar_Perfis_Com_Falha_Sem_Permissao_Admin()
        {
            var response = await _httpCliente.GetAsync("/api/v1/perfil");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact(DisplayName = "Criar Usuário Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Criar_Usuario_Com_Sucesso()
        {
            var email = $"{Guid.NewGuid():N}@usuario.novo.com";

            var response = await _httpAdmin.PostAsJsonAsync("/api/v1/usuario", new
            {
                email,
                senha = SenhaPadrao,
                perfilId = PerfilFuncionario,
                ativo = true
            });

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact(DisplayName = "Criar Usuário Com Falha Quando E-mail Inválido")]
        [Trait("Integration", "WebApi")]
        public async Task Criar_Usuario_Com_Falha_Email_Invalido()
        {
            var response = await _httpAdmin.PostAsJsonAsync("/api/v1/usuario", new
            {
                email = "",
                senha = SenhaPadrao,
                perfilId = PerfilFuncionario,
                ativo = true
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact(DisplayName = "Listar Usuários Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Listar_Usuarios_Com_Sucesso()
        {
            var response = await _httpAdmin.GetAsync("/api/v1/usuario?numeroPagina=1&tamanhoPagina=5");

            response.EnsureSuccessStatusCode();
        }

        [Fact(DisplayName = "Listar Usuários Com Falha Quando Ordenação Inválida")]
        [Trait("Integration", "WebApi")]
        public async Task Listar_Usuarios_Com_Falha_Ordenacao()
        {
            var response = await _httpAdmin.GetAsync("/api/v1/usuario?ordenarPor=CampoInvalido");

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact(DisplayName = "Obter Usuário Por Id Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Obter_Usuario_Por_Id_Com_Sucesso()
        {
            var email = $"{Guid.NewGuid():N}@usuario.getbyid.com";

            var post = await _httpAdmin.PostAsJsonAsync("/api/v1/usuario", new
            {
                email,
                senha = SenhaPadrao,
                perfilId = PerfilFuncionario,
                ativo = true
            });
            post.EnsureSuccessStatusCode();
            var userId = await post.Content.ReadFromJsonAsync<Guid>();

            var response = await _httpAdmin.GetAsync($"/api/v1/usuario/{userId}");

            response.EnsureSuccessStatusCode();
        }

        [Fact(DisplayName = "Obter Usuário Por Id Com Falha Quando Não Existir")]
        [Trait("Integration", "WebApi")]
        public async Task Obter_Usuario_Por_Id_Com_Falha()
        {
            var response = await _httpAdmin.GetAsync($"/api/v1/usuario/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "Ativar Usuário Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Ativar_Usuario_Com_Sucesso()
        {
            var email = $"{Guid.NewGuid():N}@usuario.inativo.com";

            var post = await _httpAdmin.PostAsJsonAsync("/api/v1/usuario", new
            {
                email,
                senha = SenhaPadrao,
                perfilId = PerfilFuncionario,
                ativo = false
            });
            post.EnsureSuccessStatusCode();
            var userId = await post.Content.ReadFromJsonAsync<Guid>();

            var response = await _httpAdmin.PutAsync(
                $"/api/v1/usuario/{userId}/ativar",
                new StringContent(string.Empty, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact(DisplayName = "Ativar Usuário Com Falha Quando Não Existir")]
        [Trait("Integration", "WebApi")]
        public async Task Ativar_Usuario_Com_Falha()
        {
            var response = await _httpAdmin.PutAsync(
                $"/api/v1/usuario/{Guid.NewGuid()}/ativar",
                new StringContent(string.Empty, Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "Inativar Usuário Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Inativar_Usuario_Com_Sucesso()
        {
            var email = $"{Guid.NewGuid():N}@usuario.del.com";

            var post = await _httpAdmin.PostAsJsonAsync("/api/v1/usuario", new
            {
                email,
                senha = SenhaPadrao,
                perfilId = PerfilFuncionario,
                ativo = true
            });
            post.EnsureSuccessStatusCode();
            var userId = await post.Content.ReadFromJsonAsync<Guid>();

            var response = await _httpAdmin.DeleteAsync($"/api/v1/usuario/{userId}");

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact(DisplayName = "Inativar Usuário Com Falha Quando Não Existir")]
        [Trait("Integration", "WebApi")]
        public async Task Inativar_Usuario_Com_Falha()
        {
            var response = await _httpAdmin.DeleteAsync($"/api/v1/usuario/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "Resetar Senha Do Usuário Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Resetar_Senha_Usuario_Com_Sucesso()
        {
            var email = $"{Guid.NewGuid():N}@usuario.reset.com";

            var post = await _httpAdmin.PostAsJsonAsync("/api/v1/usuario", new
            {
                email,
                senha = SenhaPadrao,
                perfilId = PerfilFuncionario,
                ativo = true
            });
            post.EnsureSuccessStatusCode();
            var userId = await post.Content.ReadFromJsonAsync<Guid>();

            var response = await _httpAdmin.PutAsync(
                $"/api/v1/usuario/{userId}/resetar-acesso",
                new StringContent(string.Empty, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact(DisplayName = "Resetar Senha Do Usuário Com Falha Quando Não Existir")]
        [Trait("Integration", "WebApi")]
        public async Task Resetar_Senha_Usuario_Com_Falha()
        {
            var response = await _httpAdmin.PutAsync(
                $"/api/v1/usuario/{Guid.NewGuid()}/resetar-acesso",
                new StringContent(string.Empty, Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "Primeiro Acesso Definindo Senha Com Sucesso")]
        [Trait("Integration", "WebApi")]
        public async Task Primeiro_Acesso_Definir_Senha_Com_Sucesso()
        {
            var email = $"{Guid.NewGuid():N}@usuario.primeiro.com";

            using (var scope = _factory.Services.CreateScope())
            {
                var mediator = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();
                await mediator.Send(new CriarUsuarioCommand(email, null, PerfilFuncionario, true, requerSenha: false));
            }

            var httpSemAuth = _factory.CreateClient();
            var novaSenha = "TerceiraSenhaLonga9#Qm";

            var response = await httpSemAuth.PutAsJsonAsync("/api/v1/usuario/primeiro-acesso", new
            {
                email,
                senha = novaSenha
            });

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact(DisplayName = "Primeiro Acesso Com Falha Quando Usuário Não Existir")]
        [Trait("Integration", "WebApi")]
        public async Task Primeiro_Acesso_Com_Falha_Email_Inexistente()
        {
            var httpSemAuth = _factory.CreateClient();

            var response = await httpSemAuth.PutAsJsonAsync("/api/v1/usuario/primeiro-acesso", new
            {
                email = "naoexiste@teste000.com",
                senha = "QuartaSenhaLonga9#Qm"
            });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
