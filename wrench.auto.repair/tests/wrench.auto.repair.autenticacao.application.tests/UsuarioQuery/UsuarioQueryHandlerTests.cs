using Moq;
using Moq.AutoMock;
using System.Linq.Expressions;
using wrench.auto.repair.autenticacao.application.Paginacao;
using wrench.auto.repair.autenticacao.application.Queries;
using wrench.auto.repair.autenticacao.application.tests.Fixtures;
using wrench.auto.repair.autenticacao.domain.Data;
using wrench.auto.repair.autenticacao.domain.Entities;
using wrench.auto.repair.core.Pagination;

namespace wrench.auto.repair.autenticacao.application.tests.UsuarioQuery
{
    [Collection(nameof(UsuarioCollection))]
    public class UsuarioQueryHandlerTests(UsuarioFixture _fixture)
    {
        [Fact(DisplayName = "Listar Todos Os Perfis Com Sucesso")]
        [Trait("Autenticacao", "Application")]
        public async Task Perfil_ListarTodosPerfis_DeveRetornarPerfis()
        {
            var obterPerfisQuery = new ObterTodosPerfisQuery();
            var autoMocker = new AutoMocker();
            var usuarioQueryHandler = autoMocker.CreateInstance<UsuarioQueryHandler>();
            autoMocker.GetMock<IUsuarioRepository>().
                Setup(u => u.ObterTodosPerfisAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IEnumerable<Perfil>>([]));

            var result = await usuarioQueryHandler
                .Handle(obterPerfisQuery, CancellationToken.None);

            Assert.True(result.Sucesso);
            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.ObterTodosPerfisAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "Obter Todos Os Usuários com sucesso")]
        [Trait("Autenticacao", "Application")]
        public async Task Usuarios_ObterTodos_DeveRetornarComSucesso()
        {
            var resultadoPaginadoUsuario = new ResultadoPaginado<Usuario>([], 0, 1, 10);
            var obterTodosUsuariosQuery = new ObterTodosUsuariosQuery(new UsuarioRequisicaoPaginada());
            var autoMocker = new AutoMocker();
            var usuarioQueryHandler = autoMocker.CreateInstance<UsuarioQueryHandler>();

            autoMocker.GetMock<IUsuarioRepository>().
                Setup(u => u.BuscaPaginadaAsync(It.IsAny<UsuarioRequisicaoPaginada>(), It.IsAny<Dictionary<string, Expression<Func<Usuario, object?>>>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<ResultadoPaginado<Usuario>>(resultadoPaginadoUsuario));

            var result = await usuarioQueryHandler.Handle(obterTodosUsuariosQuery, CancellationToken.None);

            Assert.True(result.Sucesso);
            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.BuscaPaginadaAsync(It.IsAny<UsuarioRequisicaoPaginada>(), It.IsAny<Dictionary<string, Expression<Func<Usuario, object?>>>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "Obter Usuário Por Identificador Com Sucesso")]
        [Trait("Autenticacao", "Application")]
        public async Task Usuario_ObterPorIdentificador_DeveRetornarUsuario()
        {
            var usuario = _fixture.GerarUsuario();
            var obterUsuarioPorIdQuery = new ObterUsuarioPorIdQuery(usuario.Id);
            var autoMocker = new AutoMocker();
            var usuarioQueryHandler = autoMocker.CreateInstance<UsuarioQueryHandler>();
            autoMocker.GetMock<IUsuarioRepository>().
                Setup(u => u.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Usuario?>(usuario));

            var result = await usuarioQueryHandler
                .Handle(obterUsuarioPorIdQuery, CancellationToken.None);

            Assert.True(result.Sucesso);
            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "Obter Usuário Por Id Usuario Nao Encontrado")]
        [Trait("Autenticacao", "Application")]
        public async Task Usuario_ObterPorId_UsuarioNaoEncontrado()
        {
            var obterUsuarioPorIdQuery = new ObterUsuarioPorIdQuery(Guid.NewGuid());
            var autoMocker = new AutoMocker();
            var usuarioQueryHandler = autoMocker.CreateInstance<UsuarioQueryHandler>();
            autoMocker.GetMock<IUsuarioRepository>().
                Setup(u => u.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Usuario?>(null));

            var result = await usuarioQueryHandler
                .Handle(obterUsuarioPorIdQuery, CancellationToken.None);

            Assert.False(result.Sucesso);
            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "Listar perfis deve retornar não encontrado quando repositório retornar nulo")]
        [Trait("Autenticacao", "Application")]
        public async Task Perfil_ListarTodosPerfis_DeveRetornarNotFound_QuandoRepositorioRetornarNulo()
        {
            var query = new ObterTodosPerfisQuery();
            var autoMocker = new AutoMocker();
            var handler = autoMocker.CreateInstance<UsuarioQueryHandler>();

            autoMocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.ObterTodosPerfisAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IEnumerable<Perfil>>(null!));

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "Listar usuários deve retornar não encontrado quando paginação retornar nulo")]
        [Trait("Autenticacao", "Application")]
        public async Task Usuarios_ObterTodos_DeveRetornarNotFound_QuandoRepositorioRetornarNulo()
        {
            var query = new ObterTodosUsuariosQuery(new UsuarioRequisicaoPaginada());
            var autoMocker = new AutoMocker();
            var handler = autoMocker.CreateInstance<UsuarioQueryHandler>();

            autoMocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.BuscaPaginadaAsync(It.IsAny<UsuarioRequisicaoPaginada>(), It.IsAny<Dictionary<string, Expression<Func<Usuario, object?>>>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<ResultadoPaginado<Usuario>>(null!));

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "Listar usuários deve retornar erro de validação quando ordenação for inválida")]
        [Trait("Autenticacao", "Application")]
        public async Task Usuarios_ObterTodos_DeveRetornarUnprocessableEntity_QuandoOrdenacaoInvalida()
        {
            var paginacao = new UsuarioRequisicaoPaginada { OrdenarPor = "CampoInexistente" };
            var query = new ObterTodosUsuariosQuery(paginacao);
            var autoMocker = new AutoMocker();
            var handler = autoMocker.CreateInstance<UsuarioQueryHandler>();

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.False(result.Sucesso);
            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.BuscaPaginadaAsync(It.IsAny<UsuarioRequisicaoPaginada>(), It.IsAny<Dictionary<string, Expression<Func<Usuario, object?>>>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Obter usuário por id deve retornar validação quando id for vazio")]
        [Trait("Autenticacao", "Application")]
        public async Task Usuario_ObterPorId_DeveRetornarErro_QuandoIdInvalido()
        {
            var query = new ObterUsuarioPorIdQuery(Guid.Empty);
            var autoMocker = new AutoMocker();
            var handler = autoMocker.CreateInstance<UsuarioQueryHandler>();

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.False(result.Sucesso);
            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
