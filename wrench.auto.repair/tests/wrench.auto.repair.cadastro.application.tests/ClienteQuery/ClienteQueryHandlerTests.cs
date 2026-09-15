using AutoMapper;
using Moq;
using Moq.AutoMock;
using System.Linq.Expressions;
using wrench.auto.repair.cadastro.application.Paginacao;
using wrench.auto.repair.cadastro.application.Queries;
using wrench.auto.repair.cadastro.application.Queries.ViewModels;
using wrench.auto.repair.cadastro.application.tests.Fixture;
using wrench.auto.repair.cadastro.domain.Data;
using wrench.auto.repair.cadastro.domain.Entities;
using wrench.auto.repair.core.Pagination;

namespace wrench.auto.repair.cadastro.application.tests.ClienteQuery
{
    [Collection(nameof(ClienteCollection))]
    public class ClienteQueryHandlerTests(ClienteFixture _fixture)
    {
        [Fact(DisplayName = "Buscar Todos Os Clientes Com Sucesso")]
        [Trait("Cadastro", "Application")]
        public async Task Cliente_ListarTodosClientes_DeveRetornarListaComSucesso()
        {
            var paginacao = new ClienteRequisicaoPaginada();
            var resultadoPaginadoCliente =
                new ResultadoPaginado<Cliente>([], 0, 1, 10);

            var resultadoPaginadoClienteViewModel =
                new ResultadoPaginado<ClienteViewModel>([], 0, 1, 10);

            var obterTodosOsClientesQuery = new ObterTodosClientesQuery(paginacao);
            var mapper = _fixture.ConfigurarMapeamentoEGerarMapper();
            var automocker = new AutoMocker();
            var clienteQueryHandler = automocker.CreateInstance<ClienteQueryHandler>();

            automocker.GetMock<IClienteRepository>()
                .Setup(c => c.BuscaPaginadaAsync(paginacao, It.IsAny<Dictionary<string, Expression<Func<Cliente, object?>>>>(), CancellationToken.None))
                .Returns(Task.FromResult<ResultadoPaginado<Cliente>>(resultadoPaginadoCliente));

            automocker.GetMock<IMapper>()
                .Setup(c => c.Map<ResultadoPaginado<ClienteViewModel>>(It.IsAny<ResultadoPaginado<Cliente>>()))
                .Returns(resultadoPaginadoClienteViewModel);

            var result = await clienteQueryHandler.Handle(obterTodosOsClientesQuery, CancellationToken.None);

            Assert.True(result.Sucesso);

            automocker.GetMock<IClienteRepository>()
                .Verify(c => c.BuscaPaginadaAsync(paginacao, It.IsAny<Dictionary<string, Expression<Func<Cliente, object?>>>>(), CancellationToken.None), Times.Once);

            automocker.GetMock<IMapper>()
                .Verify(c => c.Map<ResultadoPaginado<ClienteViewModel>>(It.IsAny<ResultadoPaginado<Cliente>>()), Times.Once);
        }

        [Fact(DisplayName = "Buscar Todos Os Clientes Falha Ao Consultar")]
        [Trait("Cadastro", "Application")]
        public async Task Cliente_ListarTodosClientes_FalhaAoConsultar()
        {
            var paginacao = new ClienteRequisicaoPaginada();
            var resultadoPaginadoCliente =
                new ResultadoPaginado<Cliente>([], 0, 1, 10);

            var resultadoPaginadoClienteViewModel =
                new ResultadoPaginado<ClienteViewModel>([], 0, 1, 10);

            var obterTodosOsClientesQuery = new ObterTodosClientesQuery(paginacao);
            var mapper = _fixture.ConfigurarMapeamentoEGerarMapper();
            var automocker = new AutoMocker();
            var clienteQueryHandler = automocker.CreateInstance<ClienteQueryHandler>();

            automocker.GetMock<IClienteRepository>()
                .Setup(c => c.BuscaPaginadaAsync(paginacao, It.IsAny<Dictionary<string, Expression<Func<Cliente, object?>>>>(), CancellationToken.None))
                .Returns(Task.FromResult<ResultadoPaginado<Cliente>>(null!));

            var result = await clienteQueryHandler.Handle(obterTodosOsClientesQuery, CancellationToken.None);

            Assert.False(result.Sucesso);

            automocker.GetMock<IClienteRepository>()
                .Verify(c => c.BuscaPaginadaAsync(paginacao, It.IsAny<Dictionary<string, Expression<Func<Cliente, object?>>>>(), CancellationToken.None), Times.Once);

            automocker.GetMock<IMapper>()
                .Verify(c => c.Map<ResultadoPaginado<ClienteViewModel>>(It.IsAny<ResultadoPaginado<Cliente>>()), Times.Never);
        }

        [Fact(DisplayName = "Buscar Cliente Por Id Com Sucesso")]
        [Trait("Cadastro", "Application")]
        public async Task Cliente_BuscarClientePorId_DeveRetornarClienteSolicitado()
        {
            var mapper = _fixture.ConfigurarMapeamentoEGerarMapper();
            var cliente = _fixture.GerarClienteValido();
            var clienteViewModel = mapper.Map<ClienteViewModel>(cliente);
            var obterClientePorIdQuery = new ObterClientePorIdQuery(cliente.Id);
            var automocker = new AutoMocker();
            var clienteQueryHandler = automocker.CreateInstance<ClienteQueryHandler>();

            automocker.GetMock<IClienteRepository>()
                .Setup(c => c.ObterPorIdAsync(cliente.Id, CancellationToken.None))
                .Returns(Task.FromResult<Cliente?>(cliente));

            automocker.GetMock<IMapper>()
                .Setup(c => c.Map<ClienteViewModel>(It.IsAny<Cliente>()))
                .Returns(clienteViewModel);

            var result = await clienteQueryHandler.Handle(obterClientePorIdQuery, CancellationToken.None);

            Assert.True(result.Sucesso);

            automocker.GetMock<IClienteRepository>()
                .Verify(c => c.ObterPorIdAsync(cliente.Id, CancellationToken.None), Times.Once);

            automocker.GetMock<IMapper>()
                .Verify(c => c.Map<ClienteViewModel>(It.IsAny<Cliente>()), Times.Once);
        }

        [Fact(DisplayName = "Buscar Cliente Por Documento Com Sucesso")]
        [Trait("Cadastro", "Application")]
        public async Task Cliente_BuscarClientePorDocumento_DeveRetornarClienteSolicitado()
        {
            var mapper = _fixture.ConfigurarMapeamentoEGerarMapper();
            var cliente = _fixture.GerarClienteValido();
            var clienteViewModel = mapper.Map<ClienteViewModel>(cliente);
            var obterClientePorDocumentoQuery = new ObterClientePorDocumentoQuery(cliente.Documento.Numeracao);
            var automocker = new AutoMocker();
            var clienteQueryHandler = automocker.CreateInstance<ClienteQueryHandler>();

            automocker.GetMock<IClienteRepository>()
                .Setup(c => c.ObterPorDocumentAsync(cliente.Documento.Numeracao, CancellationToken.None))
                .Returns(Task.FromResult<Cliente?>(cliente));

            automocker.GetMock<IMapper>()
                .Setup(c => c.Map<ClienteViewModel>(It.IsAny<Cliente>()))
                .Returns(clienteViewModel);

            var result = await clienteQueryHandler.Handle(obterClientePorDocumentoQuery, CancellationToken.None);

            Assert.True(result.Sucesso);

            automocker.GetMock<IClienteRepository>()
                .Verify(c => c.ObterPorDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IMapper>()
                .Verify(c => c.Map<ClienteViewModel>(It.IsAny<Cliente>()), Times.Once);
        }

        [Fact(DisplayName = "Listar clientes deve retornar erro quando ordenação for inválida")]
        [Trait("Cadastro", "Application")]
        public async Task Cliente_ListarTodosClientes_DeveRetornarErro_QuandoOrdenacaoInvalida()
        {
            var paginacao = new ClienteRequisicaoPaginada { OrdenarPor = "CampoInvalido" };
            var query = new ObterTodosClientesQuery(paginacao);
            var automocker = new AutoMocker();
            var handler = automocker.CreateInstance<ClienteQueryHandler>();

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.False(result.Sucesso);
            automocker.GetMock<IClienteRepository>()
                .Verify(c => c.BuscaPaginadaAsync(It.IsAny<ClienteRequisicaoPaginada>(), It.IsAny<Dictionary<string, Expression<Func<Cliente, object?>>>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Buscar cliente por id deve retornar não encontrado quando não existir")]
        [Trait("Cadastro", "Application")]
        public async Task Cliente_BuscarClientePorId_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var query = new ObterClientePorIdQuery(Guid.NewGuid());
            var automocker = new AutoMocker();
            var handler = automocker.CreateInstance<ClienteQueryHandler>();

            automocker.GetMock<IClienteRepository>()
                .Setup(c => c.ObterPorIdAsync(query.ClienteId, CancellationToken.None))
                .Returns(Task.FromResult<Cliente?>(null));

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "Buscar cliente por id deve retornar validação quando id for vazio")]
        [Trait("Cadastro", "Application")]
        public async Task Cliente_BuscarClientePorId_DeveRetornarErro_QuandoIdInvalido()
        {
            var query = new ObterClientePorIdQuery(Guid.Empty);
            var automocker = new AutoMocker();
            var handler = automocker.CreateInstance<ClienteQueryHandler>();

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.False(result.Sucesso);
            automocker.GetMock<IClienteRepository>()
                .Verify(c => c.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Buscar cliente por documento deve retornar não encontrado quando não existir")]
        [Trait("Cadastro", "Application")]
        public async Task Cliente_BuscarClientePorDocumento_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var doc = _fixture.GerarDocumentoValido();
            var query = new ObterClientePorDocumentoQuery(doc);
            var automocker = new AutoMocker();
            var handler = automocker.CreateInstance<ClienteQueryHandler>();

            automocker.GetMock<IClienteRepository>()
                .Setup(c => c.ObterPorDocumentAsync(doc, CancellationToken.None))
                .Returns(Task.FromResult<Cliente?>(null));

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "Buscar cliente por documento deve retornar validação quando documento for inválido")]
        [Trait("Cadastro", "Application")]
        public async Task Cliente_BuscarClientePorDocumento_DeveRetornarErro_QuandoDocumentoInvalido()
        {
            var query = new ObterClientePorDocumentoQuery("123");
            var automocker = new AutoMocker();
            var handler = automocker.CreateInstance<ClienteQueryHandler>();

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.False(result.Sucesso);
            automocker.GetMock<IClienteRepository>()
                .Verify(c => c.ObterPorDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
