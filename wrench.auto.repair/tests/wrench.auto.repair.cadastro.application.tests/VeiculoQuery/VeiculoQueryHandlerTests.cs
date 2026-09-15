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
using wrench.auto.repair.core.Messages.CommonMessages.IntegratedQueries;
using wrench.auto.repair.core.Pagination;

namespace wrench.auto.repair.cadastro.application.tests.VeiculoQuery
{
    [Collection(nameof(VeiculoCollection))]
    public class VeiculoQueryHandlerTests(VeiculoFixture _fixture)
    {
        [Fact(DisplayName = "Obter Todos Veiculos Deve Retornar Veiculos")]
        [Trait("Cadastro", "Application")]
        public async Task Veiculo_ObterTodosVeiculos_DeveRetornarVeiculos()
        {
            var mapper = _fixture.ConfigurarMapeamentoEGerarMapper();
            var resultadoPaginadoVeiculo =
                new ResultadoPaginado<Veiculo>([], 0, 1, 10);
            var resultadoPaginadoVeiculoViewModel = mapper
                .Map<ResultadoPaginado<VeiculoViewModel>>(resultadoPaginadoVeiculo);

            var obterTodosVeiculosQuery =
                new ObterTodosVeiculosQuery(new VeiculoRequisicaoPaginada());
            var automocker = new AutoMocker();

            automocker.GetMock<IVeiculoRepository>()
                .Setup(v => v.BuscaPaginadaAsync(It.IsAny<VeiculoRequisicaoPaginada>(), It.IsAny<Dictionary<string, Expression<Func<Veiculo, object?>>>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<ResultadoPaginado<Veiculo>>(resultadoPaginadoVeiculo));

            automocker.GetMock<IMapper>()
                .Setup(m => m.Map<ResultadoPaginado<VeiculoViewModel>>(resultadoPaginadoVeiculo))
                .Returns(resultadoPaginadoVeiculoViewModel);

            var veiculoQueryHandler =
                automocker.CreateInstance<VeiculoQueryHandler>();

            var resultado = await veiculoQueryHandler
                .Handle(obterTodosVeiculosQuery, CancellationToken.None);

            Assert.True(resultado.Sucesso);

            automocker.GetMock<IVeiculoRepository>()
            .Verify(v => v.BuscaPaginadaAsync(It.IsAny<VeiculoRequisicaoPaginada>(), It.IsAny<Dictionary<string, Expression<Func<Veiculo, object?>>>>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IMapper>()
                .Verify(m => m.Map<ResultadoPaginado<VeiculoViewModel>>(It.IsAny<ResultadoPaginado<Veiculo>>()), Times.Once);
        }

        [Fact(DisplayName = "Obter Veiculo Por Id Deve Retornar Veiculo")]
        [Trait("Cadastro", "Application")]
        public async Task Veiculo_ObterVeiculoPorId_DeveRetornarVeiculo()
        {
            var mapper = _fixture.ConfigurarMapeamentoEGerarMapper();

            var veiculoId = Guid.NewGuid();
            var veiculo = _fixture.CriarVeiculoValido();
            var veiculoViewModel = mapper.Map<VeiculoViewModel>(veiculo);
            var obterVeiculoPorIdQuery =
                new ObterVeiculoPorIdQuery(veiculo.Id);
            var automocker = new AutoMocker();

            automocker.GetMock<IVeiculoRepository>()
                .Setup(v => v.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Veiculo?>(veiculo));

            automocker.GetMock<IMapper>()
                .Setup(m => m.Map<VeiculoViewModel>(veiculo))
                .Returns(veiculoViewModel);

            var veiculoQueryHandler =
                automocker.CreateInstance<VeiculoQueryHandler>();

            var resultado = await veiculoQueryHandler
                .Handle(obterVeiculoPorIdQuery, CancellationToken.None);

            Assert.True(resultado.Sucesso);

            automocker.GetMock<IVeiculoRepository>()
            .Verify(v => v.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IMapper>()
                .Verify(m => m.Map<VeiculoViewModel>(It.IsAny<Veiculo>()), Times.Once);
        }

        [Fact(DisplayName = "Obter Veiculo Por Placa Deve Retornar Veiculo")]
        [Trait("Cadastro", "Application")]
        public async Task Veiculo_ObterVeiculoPorPlaca_DeveRetornarVeiculo()
        {
            var mapper = _fixture.ConfigurarMapeamentoEGerarMapper();

            var veiculoId = Guid.NewGuid();
            var veiculo = _fixture.CriarVeiculoValido();
            var veiculoViewModel = mapper.Map<VeiculoViewModel>(veiculo);
            var obterVeiculoPorPlacaQuery =
                new ObterVeiculoPorPlacaQuery(veiculo.PlacaDoVeiculo);
            var automocker = new AutoMocker();

            automocker.GetMock<IVeiculoRepository>()
                .Setup(v => v.ObterVeiculoPelaPlacaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Veiculo?>(veiculo));

            automocker.GetMock<IMapper>()
                .Setup(m => m.Map<VeiculoViewModel>(veiculo))
                .Returns(veiculoViewModel);

            var veiculoQueryHandler =
                automocker.CreateInstance<VeiculoQueryHandler>();

            var resultado = await veiculoQueryHandler
                .Handle(obterVeiculoPorPlacaQuery, CancellationToken.None);

            Assert.True(resultado.Sucesso);

            automocker.GetMock<IVeiculoRepository>()
            .Verify(v => v.ObterVeiculoPelaPlacaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IMapper>()
                .Verify(m => m.Map<VeiculoViewModel>(It.IsAny<Veiculo>()), Times.Once);
        }

        [Fact(DisplayName = "Listar veículos deve retornar não encontrado quando repositório retornar nulo")]
        [Trait("Cadastro", "Application")]
        public async Task Veiculo_ObterTodos_DeveRetornarNotFound_QuandoRepositorioRetornarNulo()
        {
            var query = new ObterTodosVeiculosQuery(new VeiculoRequisicaoPaginada());
            var automocker = new AutoMocker();
            var handler = automocker.CreateInstance<VeiculoQueryHandler>();

            automocker.GetMock<IVeiculoRepository>()
                .Setup(v => v.BuscaPaginadaAsync(It.IsAny<VeiculoRequisicaoPaginada>(), It.IsAny<Dictionary<string, Expression<Func<Veiculo, object?>>>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<ResultadoPaginado<Veiculo>>(null!));

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "Listar veículos deve retornar erro quando ordenação for inválida")]
        [Trait("Cadastro", "Application")]
        public async Task Veiculo_ObterTodos_DeveRetornarErro_QuandoOrdenacaoInvalida()
        {
            var paginacao = new VeiculoRequisicaoPaginada { OrdenarPor = "Invalido" };
            var query = new ObterTodosVeiculosQuery(paginacao);
            var automocker = new AutoMocker();
            var handler = automocker.CreateInstance<VeiculoQueryHandler>();

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "Veículo existe e pertence ao cliente deve retornar sucesso")]
        [Trait("Cadastro", "Application")]
        public async Task Veiculo_VeiculoExisteEPertenceAoCliente_DeveRetornarSucesso()
        {
            var veiculo = _fixture.CriarVeiculoValido();
            var query = new VeiculoExisteEPertenteAoClienteQuery(veiculo.ClienteId, veiculo.Id);
            var automocker = new AutoMocker();
            var handler = automocker.CreateInstance<VeiculoQueryHandler>();

            automocker.GetMock<IVeiculoRepository>()
                .Setup(v => v.ObterPorIdAsync(veiculo.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Veiculo?>(veiculo));

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.True(result.Sucesso);
        }

        [Fact(DisplayName = "Veículo existe query deve retornar não encontrado quando veículo não existir")]
        [Trait("Cadastro", "Application")]
        public async Task Veiculo_VeiculoExisteEPertenceAoCliente_DeveRetornarNotFound_QuandoVeiculoNaoExistir()
        {
            var query = new VeiculoExisteEPertenteAoClienteQuery(Guid.NewGuid(), Guid.NewGuid());
            var automocker = new AutoMocker();
            var handler = automocker.CreateInstance<VeiculoQueryHandler>();

            automocker.GetMock<IVeiculoRepository>()
                .Setup(v => v.ObterPorIdAsync(query.VeiculoId, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Veiculo?>(null));

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "Veículo existe query deve retornar não encontrado quando veículo não pertencer ao cliente")]
        [Trait("Cadastro", "Application")]
        public async Task Veiculo_VeiculoExisteEPertenceAoCliente_DeveRetornarNotFound_QuandoClienteDiferente()
        {
            var veiculo = _fixture.CriarVeiculoValido();
            var query = new VeiculoExisteEPertenteAoClienteQuery(Guid.NewGuid(), veiculo.Id);
            var automocker = new AutoMocker();
            var handler = automocker.CreateInstance<VeiculoQueryHandler>();

            automocker.GetMock<IVeiculoRepository>()
                .Setup(v => v.ObterPorIdAsync(veiculo.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Veiculo?>(veiculo));

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "Obter veículo por id deve retornar validação quando id for vazio")]
        [Trait("Cadastro", "Application")]
        public async Task Veiculo_ObterPorId_DeveRetornarErro_QuandoIdInvalido()
        {
            var query = new ObterVeiculoPorIdQuery(Guid.Empty);
            var automocker = new AutoMocker();
            var handler = automocker.CreateInstance<VeiculoQueryHandler>();

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.False(result.Sucesso);
            automocker.GetMock<IVeiculoRepository>()
                .Verify(v => v.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Obter veículo por placa deve retornar validação quando placa for inválida")]
        [Trait("Cadastro", "Application")]
        public async Task Veiculo_ObterPorPlaca_DeveRetornarErro_QuandoPlacaInvalida()
        {
            var query = new ObterVeiculoPorPlacaQuery("XXX");
            var automocker = new AutoMocker();
            var handler = automocker.CreateInstance<VeiculoQueryHandler>();

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.False(result.Sucesso);
            automocker.GetMock<IVeiculoRepository>()
                .Verify(v => v.ObterVeiculoPelaPlacaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
