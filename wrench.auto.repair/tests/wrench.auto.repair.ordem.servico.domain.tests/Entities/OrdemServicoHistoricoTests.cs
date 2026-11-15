using wrench.auto.repair.core.DomainObjects;
using wrench.auto.repair.core.Services;
using wrench.auto.repair.ordem.servico.domain.Entities;
using wrench.auto.repair.ordem.servico.domain.Enums;

namespace wrench.auto.repair.ordem.servico.domain.tests.Entities
{
    public class OrdemServicoHistoricoTests
    {
        [Fact(DisplayName = "Nova ordem deve registrar a entrada inicial no histórico")]
        [Trait("Ordem Serviço", "Domain")]
        public void Construtor_DeveRegistrarEntradaInicial()
        {
            var ordem = CriarEm(OrdemServicoStatus.Recebida);

            var entrada = Assert.Single(ordem.Historico);
            Assert.Null(entrada.StatusAnterior);
            Assert.Equal(OrdemServicoStatus.Recebida, entrada.StatusNovo);
            Assert.Null(entrada.Motivo);
        }

        [Fact(DisplayName = "Fluxo completo deve registrar cada transição em ordem")]
        [Trait("Ordem Serviço", "Domain")]
        public void FluxoCompleto_DeveRegistrarTodasAsTransicoes()
        {
            var ordem = CriarEm(OrdemServicoStatus.Entregue);

            var esperado = new (OrdemServicoStatus? Anterior, OrdemServicoStatus Novo)[]
            {
                (null, OrdemServicoStatus.Recebida),
                (OrdemServicoStatus.Recebida, OrdemServicoStatus.EmDiagnostico),
                (OrdemServicoStatus.EmDiagnostico, OrdemServicoStatus.AguardandoAprovacao),
                (OrdemServicoStatus.AguardandoAprovacao, OrdemServicoStatus.EmExecucao),
                (OrdemServicoStatus.EmExecucao, OrdemServicoStatus.Finalizada),
                (OrdemServicoStatus.Finalizada, OrdemServicoStatus.Entregue)
            };

            var atual = ordem.Historico.Select(h => (h.StatusAnterior, h.StatusNovo)).ToArray();

            Assert.Equal(esperado, atual);
        }

        [Fact(DisplayName = "Recusar orçamento deve registrar o motivo no histórico")]
        [Trait("Ordem Serviço", "Domain")]
        public void RecusarOrcamento_DeveRegistrarMotivo()
        {
            var ordem = CriarEm(OrdemServicoStatus.AguardandoAprovacao);

            ordem.RecusarOrcamento("Valor acima do esperado");

            var ultima = ordem.Historico.Last();
            Assert.Equal(OrdemServicoStatus.AguardandoAprovacao, ultima.StatusAnterior);
            Assert.Equal(OrdemServicoStatus.Finalizada, ultima.StatusNovo);
            Assert.Equal("Valor acima do esperado", ultima.Motivo);
        }

        [Theory(DisplayName = "Cancelar deve funcionar nos status que permitem cancelamento")]
        [Trait("Ordem Serviço", "Domain")]
        [InlineData(OrdemServicoStatus.Recebida)]
        [InlineData(OrdemServicoStatus.EmDiagnostico)]
        [InlineData(OrdemServicoStatus.AguardandoAprovacao)]
        [InlineData(OrdemServicoStatus.EmExecucao)]
        public void Cancelar_DeveCancelar_QuandoStatusPermite(OrdemServicoStatus statusInicial)
        {
            var ordem = CriarEm(statusInicial);

            ordem.Cancelar("Pagamento recusado");

            Assert.Equal(OrdemServicoStatus.Cancelada, ordem.Status);
            var ultima = ordem.Historico.Last();
            Assert.Equal(statusInicial, ultima.StatusAnterior);
            Assert.Equal(OrdemServicoStatus.Cancelada, ultima.StatusNovo);
            Assert.Equal("Pagamento recusado", ultima.Motivo);
        }

        [Theory(DisplayName = "Cancelar deve falhar nos status que não permitem cancelamento")]
        [Trait("Ordem Serviço", "Domain")]
        [InlineData(OrdemServicoStatus.Finalizada)]
        [InlineData(OrdemServicoStatus.Entregue)]
        [InlineData(OrdemServicoStatus.Cancelada)]
        public void Cancelar_DeveLancar_QuandoStatusNaoPermite(OrdemServicoStatus statusInicial)
        {
            var ordem = CriarEm(statusInicial);

            Assert.False(ordem.PodeSerCancelada());
            Assert.Throws<DomainException>(() => ordem.Cancelar("Motivo"));
        }

        [Fact(DisplayName = "Cancelar deve exigir motivo")]
        [Trait("Ordem Serviço", "Domain")]
        public void Cancelar_DeveLancar_QuandoMotivoVazio()
        {
            var ordem = CriarEm(OrdemServicoStatus.EmExecucao);

            Assert.Throws<DomainException>(() => ordem.Cancelar("  "));
            Assert.Equal(OrdemServicoStatus.EmExecucao, ordem.Status);
        }

        [Fact(DisplayName = "Retornar para diagnóstico deve voltar de aguardando aprovação e registrar a falha")]
        [Trait("Ordem Serviço", "Domain")]
        public void RetornarParaDiagnostico_DeveVoltarDeAguardandoAprovacao()
        {
            var ordem = CriarEm(OrdemServicoStatus.AguardandoAprovacao);

            ordem.RetornarParaDiagnostico("Falha ao gerar orçamento");

            Assert.Equal(OrdemServicoStatus.EmDiagnostico, ordem.Status);
            Assert.Equal(StatusAprovacaoEnum.Indefinido, ordem.StatusAprovacao);
            var ultima = ordem.Historico.Last();
            Assert.Equal(OrdemServicoStatus.AguardandoAprovacao, ultima.StatusAnterior);
            Assert.Equal(OrdemServicoStatus.EmDiagnostico, ultima.StatusNovo);
            Assert.Equal("Falha ao gerar orçamento", ultima.Motivo);
        }

        [Fact(DisplayName = "Retornar para diagnóstico em diagnóstico deve só registrar a falha")]
        [Trait("Ordem Serviço", "Domain")]
        public void RetornarParaDiagnostico_DeveRegistrarFalha_QuandoJaEmDiagnostico()
        {
            var ordem = CriarEm(OrdemServicoStatus.EmDiagnostico);
            var totalAntes = ordem.Historico.Count;

            ordem.RetornarParaDiagnostico("Timeout no orçamento");

            Assert.Equal(OrdemServicoStatus.EmDiagnostico, ordem.Status);
            Assert.Equal(totalAntes + 1, ordem.Historico.Count);
            Assert.Equal("Timeout no orçamento", ordem.Historico.Last().Motivo);
        }

        [Theory(DisplayName = "Retornar para diagnóstico deve falhar fora de diagnóstico e aguardando aprovação")]
        [Trait("Ordem Serviço", "Domain")]
        [InlineData(OrdemServicoStatus.Recebida)]
        [InlineData(OrdemServicoStatus.EmExecucao)]
        [InlineData(OrdemServicoStatus.Finalizada)]
        [InlineData(OrdemServicoStatus.Cancelada)]
        public void RetornarParaDiagnostico_DeveLancar_QuandoStatusNaoPermite(OrdemServicoStatus statusInicial)
        {
            var ordem = CriarEm(statusInicial);

            Assert.Throws<DomainException>(() => ordem.RetornarParaDiagnostico("Motivo"));
        }

        [Fact(DisplayName = "CorrelationId explícito deve ser gravado no histórico")]
        [Trait("Ordem Serviço", "Domain")]
        public void Transicao_DeveGravarCorrelationIdExplicito()
        {
            var ordem = CriarEm(OrdemServicoStatus.Recebida);

            ordem.SolicitaDiagnostico("corr-123");

            Assert.Equal("corr-123", ordem.Historico.Last().CorrelationId);
        }

        [Fact(DisplayName = "CorrelationId do contexto deve ser usado quando não informado")]
        [Trait("Ordem Serviço", "Domain")]
        public void Transicao_DeveUsarCorrelationIdDoContexto()
        {
            CorrelationContext.Current = "corr-contexto";
            try
            {
                var ordem = CriarEm(OrdemServicoStatus.Recebida);
                ordem.SolicitaDiagnostico();

                Assert.All(ordem.Historico, h => Assert.Equal("corr-contexto", h.CorrelationId));
            }
            finally
            {
                CorrelationContext.Current = null;
            }
        }

        [Fact(DisplayName = "Sem CorrelationId no contexto a entrada deve ficar sem correlação")]
        [Trait("Ordem Serviço", "Domain")]
        public void Transicao_DeveFicarSemCorrelationId_QuandoNaoHaContexto()
        {
            CorrelationContext.Current = null;

            var ordem = CriarEm(OrdemServicoStatus.Recebida);

            Assert.Null(ordem.Historico.Single().CorrelationId);
        }

        private static OrdemServico CriarEm(OrdemServicoStatus status)
        {
            var ordem = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "Revisão", OrdemServicoStatus.Recebida, DateTime.UtcNow);

            if (status == OrdemServicoStatus.Recebida) return ordem;
            if (status == OrdemServicoStatus.Cancelada)
            {
                ordem.Cancelar("Preparação do teste");
                return ordem;
            }

            ordem.SolicitaDiagnostico();
            if (status == OrdemServicoStatus.EmDiagnostico) return ordem;

            ordem.AdicionarDiagnostico("Pacote revisão completa", 500m);
            if (status == OrdemServicoStatus.AguardandoAprovacao) return ordem;

            ordem.AprovarOrcamento();
            if (status == OrdemServicoStatus.EmExecucao) return ordem;

            ordem.FinalizarOrdemServico();
            if (status == OrdemServicoStatus.Finalizada) return ordem;

            ordem.EntregarServico();
            return ordem;
        }
    }
}
