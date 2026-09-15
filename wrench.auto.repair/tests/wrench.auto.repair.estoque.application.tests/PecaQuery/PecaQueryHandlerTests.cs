using AutoMapper;
using Moq;
using Moq.AutoMock;
using System.Linq.Expressions;
using wrench.auto.repair.core.Pagination;
using wrench.auto.repair.estoque.application.Paginacao;
using wrench.auto.repair.estoque.application.Queries;
using wrench.auto.repair.estoque.application.Queries.ViewModels;
using wrench.auto.repair.estoque.application.tests.Fixtures;
using wrench.auto.repair.estoque.domain.Data;
using wrench.auto.repair.estoque.domain.Entities;

namespace wrench.auto.repair.estoque.application.tests.PecaQuery
{
    [Collection(nameof(PecaCollection))]
    public class PecaQueryHandlerTests(PecaFixture _fixture)
    {
        [Fact(DisplayName = "Consulta Peça Por Id Existente Com Sucesso")]
        [Trait("Estoque", "Application")]
        public async Task ConsultaPecaPorId_PecaExistente_DeveRetornarPeca()
        {
            var pecaExistente = new Peca("Pneu", "Aro 15", 100, 10, true, DateTime.UtcNow);
            var consultarPecaPorIdQuery =
                new ConsultarPecaPorIdQuery(Guid.NewGuid());
            var automocker = new AutoMocker();
            var mapper = _fixture.ConfigurarMapeamentoEGerarMapper();
            var pecaViewModel = mapper.Map<PecaViewModel>(pecaExistente);

            var pecaQueryHandler = automocker.CreateInstance<PecaQueryHandler>();

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Peca?>(pecaExistente));

            automocker.GetMock<IMapper>()
                .Setup(m => m.Map<PecaViewModel>(pecaExistente))
                .Returns(pecaViewModel);

            var resultado = await pecaQueryHandler
                .Handle(consultarPecaPorIdQuery, CancellationToken.None);

            Assert.Equal(pecaViewModel, resultado.Valor);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IMapper>()
                .Verify(m => m.Map<PecaViewModel>(It.IsAny<Peca>()), Times.Once);
        }

        [Fact(DisplayName = "Consulta Peça Por Id Inexistente")]
        [Trait("Estoque", "Application")]
        public async Task ConsultaPecaPorId_PecaInexistente_DeveRetornarErro()
        {
            var peca = new Peca("Pneu", "Aro 15", 100, 10, true, DateTime.UtcNow);
            var consultarPecaPorIdQuery =
                new ConsultarPecaPorIdQuery(Guid.NewGuid());
            var automocker = new AutoMocker();
            var mapper = _fixture.ConfigurarMapeamentoEGerarMapper();
            var pecaViewModel = mapper.Map<PecaViewModel>(peca);

            var pecaQueryHandler = automocker.CreateInstance<PecaQueryHandler>();

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Peca?>(null));

            automocker.GetMock<IMapper>()
                .Setup(m => m.Map<PecaViewModel>(peca))
                .Returns(pecaViewModel);

            var resultado = await pecaQueryHandler
                .Handle(consultarPecaPorIdQuery, CancellationToken.None);

            Assert.False(resultado.Sucesso);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IMapper>()
                .Verify(m => m.Map<PecaViewModel>(It.IsAny<Peca>()), Times.Never);
        }

        [Fact(DisplayName = "Obter Todas Peças Com Sucesso")]
        [Trait("Estoque", "Application")]
        public async Task Peca_ObterTodasPecas_DeveRetornarPecas()
        {
            var peca = new Peca("Pneu Aro 14", "Aro 15", 100, 10, true, DateTime.UtcNow);
            var pec2 = new Peca("Pneu Aro 16", "Aro 16", 100, 10, true, DateTime.UtcNow);
            var pecas = new List<Peca> { peca, pec2 };
            var resultadoPaginado = new ResultadoPaginado<Peca>(pecas, 2, 1, 10);

            var obterTodasPecasQuery =
                new ObterTodasPecasQuery(new PecaRequisicaoPaginada());
            var automocker = new AutoMocker();
            var mapper = _fixture.ConfigurarMapeamentoEGerarMapper();
            var resultadoPaginadoViewModel = mapper.Map<ResultadoPaginado<PecaViewModel>>(resultadoPaginado);

            var pecaQueryHandler = automocker.CreateInstance<PecaQueryHandler>();

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.BuscaPaginadaAsync(It.IsAny<PecaRequisicaoPaginada>(), It.IsAny<Dictionary<string, Expression<Func<Peca, object?>>>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<ResultadoPaginado<Peca>>(resultadoPaginado));

            automocker.GetMock<IMapper>()
                .Setup(m => m.Map<ResultadoPaginado<PecaViewModel>>(resultadoPaginado))
                .Returns(resultadoPaginadoViewModel);

            var resultado = await pecaQueryHandler
                .Handle(obterTodasPecasQuery, CancellationToken.None);

            Assert.Equal(resultadoPaginadoViewModel, resultado.Valor);

            automocker.GetMock<IPecaRepository>()
                .Verify(u => u.BuscaPaginadaAsync(It.IsAny<PecaRequisicaoPaginada>(), It.IsAny<Dictionary<string, Expression<Func<Peca, object?>>>>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IMapper>()
                .Verify(m => m.Map<ResultadoPaginado<PecaViewModel>>(It.IsAny<ResultadoPaginado<Peca>>()), Times.Once);
        }

        [Fact(DisplayName = "Consulta peça por id vazio deve falhar na validação")]
        [Trait("Estoque", "Application")]
        public async Task ConsultaPecaPorId_IdVazio_DeveRetornarValidacao()
        {
            var query = new ConsultarPecaPorIdQuery(Guid.Empty);
            var automocker = new AutoMocker();
            var handler = automocker.CreateInstance<PecaQueryHandler>();

            var resultado = await handler.Handle(query, CancellationToken.None);

            Assert.False(resultado.Sucesso);
            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Obter todas peças com ordenação inválida deve falhar na validação")]
        [Trait("Estoque", "Application")]
        public async Task ObterTodasPecas_OrdenacaoInvalida_DeveRetornarValidacao()
        {
            var pag = new PecaRequisicaoPaginada { OrdenarPor = "CampoInexistente" };
            var query = new ObterTodasPecasQuery(pag);
            var automocker = new AutoMocker();
            var handler = automocker.CreateInstance<PecaQueryHandler>();

            var resultado = await handler.Handle(query, CancellationToken.None);

            Assert.False(resultado.Sucesso);
            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.BuscaPaginadaAsync(It.IsAny<PecaRequisicaoPaginada>(), It.IsAny<Dictionary<string, Expression<Func<Peca, object?>>>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Obter todas peças quando repositório retornar nulo deve falhar")]
        [Trait("Estoque", "Application")]
        public async Task ObterTodasPecas_RepositorioNulo_DeveRetornarNaoEncontrado()
        {
            var query = new ObterTodasPecasQuery(new PecaRequisicaoPaginada { OrdenarPor = "Nome" });
            var automocker = new AutoMocker();
            var handler = automocker.CreateInstance<PecaQueryHandler>();

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.BuscaPaginadaAsync(It.IsAny<PecaRequisicaoPaginada>(), It.IsAny<Dictionary<string, Expression<Func<Peca, object?>>>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<ResultadoPaginado<Peca>>(null!));

            var resultado = await handler.Handle(query, CancellationToken.None);

            Assert.False(resultado.Sucesso);
        }
    }
}
