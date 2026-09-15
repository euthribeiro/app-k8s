using System.Diagnostics.Metrics;
using wrench.auto.repair.core.Errors;
using wrench.auto.repair.ordem.servico.application.Metrics;
using wrench.auto.repair.ordem.servico.application.Queries;
using wrench.auto.repair.ordem.servico.application.Queries.ViewModels;
using wrench.auto.repair.ordem.servico.application.UseCases.OrdemServicoUseCase;

namespace wrench.auto.repair.ordem.servico.application.tests.Metrics
{
    /// <summary>
    /// Verifica quando o pipeline incrementa o contador de falhas de processamento de ordem de serviço.
    /// Cada teste usa um identificador de ordem próprio e filtra as medições pelo nome do comando, para não
    /// sofrer interferência de outros testes que publicam no mesmo meter estático.
    /// </summary>
    public sealed class FalhaProcessamentoOrdemServicoBehaviorTests : IDisposable
    {
        private readonly MeterListener _listener = new();
        private readonly List<(long Valor, Dictionary<string, object?> Atributos)> _medicoes = [];
        private readonly Lock _trava = new();

        public FalhaProcessamentoOrdemServicoBehaviorTests()
        {
            _listener.InstrumentPublished = (instrumento, listener) =>
            {
                if (instrumento.Meter.Name == OrdemServicoMetrics.MeterName &&
                    instrumento.Name == OrdemServicoMetrics.FalhasProcessamentoNome)
                {
                    listener.EnableMeasurementEvents(instrumento);
                }
            };

            _listener.SetMeasurementEventCallback<long>((_, valor, atributos, _) =>
            {
                var mapa = new Dictionary<string, object?>();
                foreach (var atributo in atributos)
                    mapa[atributo.Key] = atributo.Value;

                lock (_trava)
                    _medicoes.Add((valor, mapa));
            });

            _listener.Start();
        }

        [Fact(DisplayName = "Deve contar falha quando o handler lança exceção e relançar a exceção")]
        [Trait("Ordem Serviço", "Metrics")]
        public async Task Handle_DeveContarFalha_QuandoHandlerLancaExcecao()
        {
            var behavior = new FalhaProcessamentoOrdemServicoBehavior<FinalizarOrdemServicoCommand, Result>();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                behavior.Handle(
                    new FinalizarOrdemServicoCommand(Guid.NewGuid()),
                    _ => throw new InvalidOperationException("banco indisponivel"),
                    CancellationToken.None));

            var medicao = Assert.Single(MedicoesDo(nameof(FinalizarOrdemServicoCommand), nameof(InvalidOperationException)));
            Assert.Equal(1, medicao.Valor);
        }

        [Fact(DisplayName = "Deve contar falha quando o handler devolve erro inesperado")]
        [Trait("Ordem Serviço", "Metrics")]
        public async Task Handle_DeveContarFalha_QuandoResultadoInesperado()
        {
            var behavior = new FalhaProcessamentoOrdemServicoBehavior<EntregarServicoCommand, Result>();

            var resultado = await behavior.Handle(
                new EntregarServicoCommand(Guid.NewGuid()),
                _ => Task.FromResult(Result.Unexpected("falha ao salvar")),
                CancellationToken.None);

            Assert.Equal(TipoErroEnum.INESPERADO, resultado.TipoErro);
            Assert.Single(MedicoesDo(nameof(EntregarServicoCommand), nameof(TipoErroEnum.INESPERADO)));
        }

        [Fact(DisplayName = "Deve contar falha quando comando genérico devolve erro inesperado")]
        [Trait("Ordem Serviço", "Metrics")]
        public async Task Handle_DeveContarFalha_QuandoResultadoGenericoInesperado()
        {
            var behavior = new FalhaProcessamentoOrdemServicoBehavior<CriarOrdemServicoCommand, Result<Guid>>();

            await behavior.Handle(
                new CriarOrdemServicoCommand(Guid.NewGuid(), Guid.NewGuid(), "Troca de oleo"),
                _ => Task.FromResult(Result<Guid>.Unexpected("falha ao salvar")),
                CancellationToken.None);

            Assert.Single(MedicoesDo(nameof(CriarOrdemServicoCommand), nameof(TipoErroEnum.INESPERADO)));
        }

        [Theory(DisplayName = "Não deve contar falha de negócio nem sucesso")]
        [Trait("Ordem Serviço", "Metrics")]
        [MemberData(nameof(ResultadosQueNaoSaoFalhaDeProcessamento))]
        public async Task Handle_NaoDeveContar_QuandoFalhaDeNegocioOuSucesso(Result resultado)
        {
            var behavior = new FalhaProcessamentoOrdemServicoBehavior<FinalizarOrdemServicoCommand, Result>();
            var comando = new FinalizarOrdemServicoCommand(Guid.NewGuid());
            var antes = MedicoesDo(nameof(FinalizarOrdemServicoCommand), nameof(TipoErroEnum.INESPERADO)).Count;

            await behavior.Handle(comando, _ => Task.FromResult(resultado), CancellationToken.None);

            Assert.Equal(antes, MedicoesDo(nameof(FinalizarOrdemServicoCommand), nameof(TipoErroEnum.INESPERADO)).Count);
        }

        [Fact(DisplayName = "Não deve contar requisições fora dos casos de uso de ordem de serviço")]
        [Trait("Ordem Serviço", "Metrics")]
        public async Task Handle_NaoDeveContar_QuandoRequisicaoForaDoContexto()
        {
            var behavior = new FalhaProcessamentoOrdemServicoBehavior<ObterOrdemServicoIdQuery, Result<OrdemServicoViewModel>>();

            await behavior.Handle(
                new ObterOrdemServicoIdQuery(Guid.NewGuid()),
                _ => Task.FromResult(Result<OrdemServicoViewModel>.Unexpected("falha")),
                CancellationToken.None);

            Assert.Empty(MedicoesDo(nameof(ObterOrdemServicoIdQuery), null));
        }

        public static TheoryData<Result> ResultadosQueNaoSaoFalhaDeProcessamento() =>
        [
            Result.Ok(),
            Result.ValidationError("invalido"),
            Result.NotFound("nao encontrada"),
            Result.Conflicted("status nao permite")
        ];

        public void Dispose() => _listener.Dispose();

        private List<(long Valor, Dictionary<string, object?> Atributos)> MedicoesDo(string comando, string? tipoErro)
        {
            lock (_trava)
            {
                return _medicoes
                    .Where(m => Equals(m.Atributos.GetValueOrDefault(OrdemServicoMetrics.AtributoComando), comando))
                    .Where(m => tipoErro is null || Equals(m.Atributos.GetValueOrDefault(OrdemServicoMetrics.AtributoTipoErro), tipoErro))
                    .ToList();
            }
        }
    }
}
