using Microsoft.EntityFrameworkCore;
using wrench.auto.repair.ordem.servico.domain.Entities;
using wrench.auto.repair.ordem.servico.domain.Enums;
using wrench.auto.repair.ordem.servico.infra.Context;

namespace wrench.auto.repair.ordem.servico.application.tests
{
    public class PersistenciaHistoricoTests
    {
        private readonly DbContextOptions<OrdemServicoDbContext> _options =
            new DbContextOptionsBuilder<OrdemServicoDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        [Fact(DisplayName = "Carregar a ordem do banco não deve criar entrada fantasma no histórico")]
        [Trait("Ordem Serviço", "Persistência")]
        public async Task Carregar_NaoDeveAdicionarEntradaNoHistorico()
        {
            var id = await CriarOrdemAsync();

            using var contexto = new OrdemServicoDbContext(_options);
            var ordem = await contexto.Set<OrdemServico>().FirstAsync(o => o.Id == id);

            var entrada = Assert.Single(ordem.Historico);
            Assert.Equal(OrdemServicoStatus.Recebida, entrada.StatusNovo);
        }

        [Fact(DisplayName = "Transições sobre ordem carregada devem acrescentar uma entrada cada ao histórico")]
        [Trait("Ordem Serviço", "Persistência")]
        public async Task Transicoes_DevemPersistirUmaEntradaCada()
        {
            var id = await CriarOrdemAsync();

            await AtualizarAsync(id, o => o.SolicitaDiagnostico("c1"));
            await AtualizarAsync(id, o => o.AdicionarDiagnostico("Pacote revisão", 100m));
            await AtualizarAsync(id, o => o.RetornarParaDiagnostico("Falha ao gerar orçamento", "c2"));
            await AtualizarAsync(id, o => o.Cancelar("Pagamento recusado", "c3"));

            using var contexto = new OrdemServicoDbContext(_options);
            var ordem = await contexto.Set<OrdemServico>().FirstAsync(o => o.Id == id);

            var historico = ordem.Historico.OrderBy(h => h.OcorridoEm).ToList();

            Assert.Equal(OrdemServicoStatus.Cancelada, ordem.Status);
            Assert.Equal(5, historico.Count);
            Assert.Equal(
                [
                    OrdemServicoStatus.Recebida,
                    OrdemServicoStatus.EmDiagnostico,
                    OrdemServicoStatus.AguardandoAprovacao,
                    OrdemServicoStatus.EmDiagnostico,
                    OrdemServicoStatus.Cancelada
                ],
                historico.Select(h => h.StatusNovo));
            Assert.Equal("Pagamento recusado", historico[^1].Motivo);
            Assert.Equal("c3", historico[^1].CorrelationId);
        }

        private async Task<Guid> CriarOrdemAsync()
        {
            using var contexto = new OrdemServicoDbContext(_options);
            var ordem = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "Revisão", OrdemServicoStatus.Recebida, DateTime.UtcNow);

            await contexto.Set<OrdemServico>().AddAsync(ordem);
            await contexto.SaveChangesAsync();

            return ordem.Id;
        }

        private async Task AtualizarAsync(Guid id, Action<OrdemServico> transicao)
        {
            using var contexto = new OrdemServicoDbContext(_options);
            var ordem = await contexto.Set<OrdemServico>().FirstAsync(o => o.Id == id);

            transicao(ordem);

            contexto.Set<OrdemServico>().Update(ordem);
            await contexto.SaveChangesAsync();
        }
    }
}
