using Microsoft.EntityFrameworkCore;
using Wrench.Contracts;
using wrench.auto.repair.ordem.servico.domain.Saga;
using wrench.auto.repair.ordem.servico.infra.Context;
using wrench.auto.repair.ordem.servico.infra.Saga;

namespace wrench.auto.repair.ordem.servico.application.tests.Saga
{
    public class PersistenciaSagaTests
    {
        private static readonly DateTime Agora = new(2026, 11, 14, 10, 0, 0, DateTimeKind.Utc);
        private static readonly TimeSpan Prazo = TimeSpan.FromMinutes(30);

        private readonly DbContextOptions<OrdemServicoDbContext> _opcoes =
            new DbContextOptionsBuilder<OrdemServicoDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        private readonly TimeProvider _relogio = new RelogioFixo(Agora);

        [Fact(DisplayName = "Saga e transições sobrevivem ao ciclo de persistência")]
        [Trait("Saga", "Persistência")]
        public async Task Saga_DevePersistirComAsTransicoes()
        {
            var ordemServicoId = Guid.NewGuid();

            using (var contexto = new OrdemServicoDbContext(_opcoes))
            {
                var saga = new SagaOrdemServico(ordemServicoId, "corr-1", Agora, Prazo);
                saga.AoCriarOrcamento(Guid.NewGuid(), Agora, Prazo);

                await new SagaOrdemServicoRepository(contexto).Adicionar(saga, CancellationToken.None);
                await contexto.CommitAsync();
            }

            using (var contexto = new OrdemServicoDbContext(_opcoes))
            {
                var saga = await new SagaOrdemServicoRepository(contexto)
                    .ObterPorOrdemServicoAsync(ordemServicoId, CancellationToken.None);

                Assert.NotNull(saga);
                Assert.Equal(SagaEstado.AguardandoAprovacao, saga.Estado);
                Assert.Equal(2, saga.Transicoes.Count);
            }
        }

        [Fact(DisplayName = "Carregar a Saga do banco não cria transição fantasma")]
        [Trait("Saga", "Persistência")]
        public async Task Carregar_NaoDeveAdicionarTransicao()
        {
            var ordemServicoId = await CriarSagaAsync();

            using var contexto = new OrdemServicoDbContext(_opcoes);
            var saga = await new SagaOrdemServicoRepository(contexto)
                .ObterPorOrdemServicoAsync(ordemServicoId, CancellationToken.None);

            Assert.Single(saga!.Transicoes);
        }

        [Fact(DisplayName = "A varredura devolve apenas as Sagas em espera com prazo vencido")]
        [Trait("Saga", "Persistência")]
        public async Task ObterExpiradas_DeveFiltrarPorPrazoEEstado()
        {
            var noPrazo = Guid.NewGuid();
            var vencida = Guid.NewGuid();
            var encerrada = Guid.NewGuid();

            using (var contexto = new OrdemServicoDbContext(_opcoes))
            {
                var repositorio = new SagaOrdemServicoRepository(contexto);

                await repositorio.Adicionar(new SagaOrdemServico(noPrazo, "corr-1", Agora, TimeSpan.FromHours(10)), CancellationToken.None);
                await repositorio.Adicionar(new SagaOrdemServico(vencida, "corr-2", Agora, TimeSpan.FromMinutes(1)), CancellationToken.None);

                var concluida = new SagaOrdemServico(encerrada, "corr-3", Agora, TimeSpan.FromMinutes(1));
                concluida.Compensar("OrcamentoFalhou", "Billing fora do ar", Agora, Prazo);
                await repositorio.Adicionar(concluida, CancellationToken.None);

                await contexto.CommitAsync();
            }

            using (var contexto = new OrdemServicoDbContext(_opcoes))
            {
                var expiradas = await new SagaOrdemServicoRepository(contexto)
                    .ObterExpiradasAsync(Agora.AddMinutes(5), 10, CancellationToken.None);

                var saga = Assert.Single(expiradas);
                Assert.Equal(vencida, saga.OrdemServicoId);
            }
        }

        [Fact(DisplayName = "Outbox grava a mensagem na transação e a entrega como pendente")]
        [Trait("Saga", "Persistência")]
        public async Task Outbox_DeveGravarEEntregarPendentes()
        {
            var ordemServicoId = Guid.NewGuid();

            using (var contexto = new OrdemServicoDbContext(_opcoes))
            {
                new OutboxSaga(contexto, _relogio).Enfileirar(
                    MensagemTipos.CancelarOrcamento,
                    new CancelarOrcamento(ordemServicoId, "corr-1", "Cliente recusou"));

                await contexto.CommitAsync();
            }

            using (var contexto = new OrdemServicoDbContext(_opcoes))
            {
                var outbox = new OutboxSaga(contexto, _relogio);

                Assert.Equal(1, await outbox.ContarPendentesAsync(CancellationToken.None));

                var pendente = Assert.Single(await outbox.ObterPendentesAsync(10, CancellationToken.None));

                Assert.Equal(MensagemTipos.CancelarOrcamento, pendente.Tipo);
                Assert.Equal(ordemServicoId, pendente.OrdemServicoId);
                Assert.Equal("corr-1", pendente.CorrelationId);
                Assert.Contains("Cliente recusou", pendente.Payload);
                Assert.Equal(Agora, pendente.CriadaEm);
            }
        }

        [Fact(DisplayName = "Mensagem publicada sai da lista de pendentes")]
        [Trait("Saga", "Persistência")]
        public async Task Outbox_MarcarPublicada_DeveRemoverDosPendentes()
        {
            using var contexto = new OrdemServicoDbContext(_opcoes);
            var outbox = new OutboxSaga(contexto, _relogio);

            outbox.Enfileirar(MensagemTipos.CancelarOrcamento,
                new CancelarOrcamento(Guid.NewGuid(), "corr-1", "Cliente recusou"));

            await contexto.CommitAsync();

            var pendente = Assert.Single(await outbox.ObterPendentesAsync(10, CancellationToken.None));

            var linha = await contexto.Set<OutboxMensagem>().FirstAsync(mensagem => mensagem.Id == pendente.Id);
            linha.PublicadaEm = Agora;
            await contexto.CommitAsync();

            Assert.Empty(await outbox.ObterPendentesAsync(10, CancellationToken.None));
            Assert.Equal(0, await outbox.ContarPendentesAsync(CancellationToken.None));
        }

        [Fact(DisplayName = "Inbox reconhece a mensagem já processada")]
        [Trait("Saga", "Persistência")]
        public async Task Inbox_DeveReconhecerMensagemRepetida()
        {
            using (var contexto = new OrdemServicoDbContext(_opcoes))
            {
                new InboxSaga(contexto).Registrar("msg-1", MensagemTipos.OrcamentoCriado, Agora);
                await contexto.CommitAsync();
            }

            using (var contexto = new OrdemServicoDbContext(_opcoes))
            {
                var inbox = new InboxSaga(contexto);

                Assert.True(await inbox.JaProcessadaAsync("msg-1", CancellationToken.None));
                Assert.False(await inbox.JaProcessadaAsync("msg-2", CancellationToken.None));
            }
        }

        private async Task<Guid> CriarSagaAsync()
        {
            var ordemServicoId = Guid.NewGuid();

            using var contexto = new OrdemServicoDbContext(_opcoes);
            await new SagaOrdemServicoRepository(contexto)
                .Adicionar(new SagaOrdemServico(ordemServicoId, "corr-1", Agora, Prazo), CancellationToken.None);
            await contexto.CommitAsync();

            return ordemServicoId;
        }

        private sealed class RelogioFixo(DateTime agora) : TimeProvider
        {
            public override DateTimeOffset GetUtcNow() => new(agora, TimeSpan.Zero);
        }
    }
}
