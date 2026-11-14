using wrench.auto.repair.core.DomainObjects;
using wrench.auto.repair.ordem.servico.domain.Saga;

namespace wrench.auto.repair.ordem.servico.domain.tests.Saga
{
    public class SagaOrdemServicoTests
    {
        private static readonly DateTime Agora = new(2026, 11, 14, 10, 0, 0, DateTimeKind.Utc);
        private static readonly TimeSpan Prazo = TimeSpan.FromMinutes(30);

        private static SagaOrdemServico Criar() =>
            new(Guid.NewGuid(), "corr-1", Agora, Prazo);

        [Fact(DisplayName = "Saga nasce aguardando o orçamento com prazo e primeira transição")]
        [Trait("Saga", "Domain")]
        public void Criar_DeveNascerAguardandoOrcamento()
        {
            var saga = Criar();

            Assert.Equal(SagaEstado.AguardandoOrcamento, saga.Estado);
            Assert.Equal(SagaEtapa.Orcamento, saga.Etapa);
            Assert.Equal(Agora.Add(Prazo), saga.PrazoEtapa);
            Assert.Single(saga.Transicoes);
            Assert.False(saga.Encerrada);
        }

        [Theory(DisplayName = "Saga exige ordem de serviço e correlação")]
        [Trait("Saga", "Domain")]
        [InlineData(true, "corr-1")]
        [InlineData(false, "")]
        public void Criar_DeveValidarEntrada(bool ordemVazia, string correlationId)
        {
            var ordemServicoId = ordemVazia ? Guid.Empty : Guid.NewGuid();

            Assert.Throws<DomainException>(() => new SagaOrdemServico(ordemServicoId, correlationId, Agora, Prazo));
        }

        [Fact(DisplayName = "Caminho feliz leva a Saga até Concluída")]
        [Trait("Saga", "Domain")]
        public void CaminhoFeliz_DeveConcluir()
        {
            var saga = Criar();

            Assert.True(saga.AoCriarOrcamento(Guid.NewGuid(), Agora, Prazo));
            Assert.Equal(SagaEstado.AguardandoAprovacao, saga.Estado);

            Assert.True(saga.AoAprovarOrcamento(Guid.NewGuid(), 500m, Agora, Prazo));
            Assert.Equal(SagaEstado.AguardandoPagamento, saga.Estado);

            Assert.True(saga.AoAprovarPagamento(Guid.NewGuid(), 500m, Agora, Prazo));
            Assert.Equal(SagaEstado.AguardandoExecucao, saga.Estado);

            Assert.True(saga.AoConcluirExecucao(Agora));
            Assert.Equal(SagaEstado.Concluida, saga.Estado);
            Assert.True(saga.Encerrada);
            Assert.Null(saga.PrazoEtapa);
            Assert.Equal(Agora, saga.DataDesfecho);
        }

        [Fact(DisplayName = "Transição repetida não muda o estado")]
        [Trait("Saga", "Domain")]
        public void TransicaoRepetida_DeveSerIgnorada()
        {
            var saga = Criar();
            var orcamentoId = Guid.NewGuid();

            Assert.True(saga.AoCriarOrcamento(orcamentoId, Agora, Prazo));
            Assert.False(saga.AoCriarOrcamento(orcamentoId, Agora, Prazo));
            Assert.Equal(2, saga.Transicoes.Count);
        }

        [Fact(DisplayName = "Transição fora de ordem é recusada")]
        [Trait("Saga", "Domain")]
        public void TransicaoForaDeOrdem_DeveSerRecusada()
        {
            var saga = Criar();

            Assert.False(saga.AoConcluirExecucao(Agora));
            Assert.False(saga.AoEstornarPagamento(Agora));
            Assert.False(saga.AoFalharEstorno("qualquer", Agora));
            Assert.Equal(SagaEstado.AguardandoOrcamento, saga.Estado);
        }

        [Fact(DisplayName = "Falha antes do pagamento encerra a compensação na hora")]
        [Trait("Saga", "Domain")]
        public void Compensar_SemPagamento_DeveEncerrar()
        {
            var saga = Criar();
            saga.AoCriarOrcamento(Guid.NewGuid(), Agora, Prazo);

            Assert.True(saga.Compensar("OrcamentoRecusado", "Cliente recusou", Agora, Prazo));

            Assert.Equal(SagaEstado.Compensada, saga.Estado);
            Assert.Equal("Cliente recusou", saga.MotivoDesfecho);
            Assert.True(saga.Encerrada);
            Assert.Null(saga.PrazoEtapa);
        }

        [Fact(DisplayName = "Falha na execução aguarda o estorno antes de encerrar")]
        [Trait("Saga", "Domain")]
        public void Compensar_ComPagamento_DeveAguardarEstorno()
        {
            var saga = Criar();
            saga.AoCriarOrcamento(Guid.NewGuid(), Agora, Prazo);
            saga.AoAprovarOrcamento(Guid.NewGuid(), 500m, Agora, Prazo);
            saga.AoAprovarPagamento(Guid.NewGuid(), 500m, Agora, Prazo);

            Assert.True(saga.Compensar("ExecucaoFalhou", "Sem mecanico disponivel", Agora, Prazo));

            Assert.Equal(SagaEstado.Compensando, saga.Estado);
            Assert.Equal(SagaEtapa.Estorno, saga.Etapa);
            Assert.Equal(Agora.Add(Prazo), saga.PrazoEtapa);
            Assert.False(saga.Encerrada);

            Assert.True(saga.AoEstornarPagamento(Agora));
            Assert.Equal(SagaEstado.Compensada, saga.Estado);
            Assert.True(saga.Encerrada);
        }

        [Fact(DisplayName = "Estorno que falha deixa a Saga em Falha para tratamento manual")]
        [Trait("Saga", "Domain")]
        public void EstornoQueFalha_DeveParar()
        {
            var saga = Criar();
            saga.AoCriarOrcamento(Guid.NewGuid(), Agora, Prazo);
            saga.AoAprovarOrcamento(Guid.NewGuid(), 500m, Agora, Prazo);
            saga.AoAprovarPagamento(Guid.NewGuid(), 500m, Agora, Prazo);
            saga.Compensar("ExecucaoFalhou", "Sem mecanico disponivel", Agora, Prazo);

            Assert.True(saga.AoFalharEstorno("Mercado Pago recusou o estorno", Agora));

            Assert.Equal(SagaEstado.Falha, saga.Estado);
            Assert.Equal("Mercado Pago recusou o estorno", saga.MotivoDesfecho);
            Assert.True(saga.Encerrada);
        }

        [Fact(DisplayName = "Saga encerrada não compensa de novo")]
        [Trait("Saga", "Domain")]
        public void Compensar_SagaEncerrada_DeveSerRecusada()
        {
            var saga = Criar();
            saga.AoCriarOrcamento(Guid.NewGuid(), Agora, Prazo);
            saga.AoAprovarOrcamento(Guid.NewGuid(), 500m, Agora, Prazo);
            saga.AoAprovarPagamento(Guid.NewGuid(), 500m, Agora, Prazo);
            saga.AoConcluirExecucao(Agora);

            Assert.False(saga.Compensar("ExecucaoFalhou", "tarde demais", Agora, Prazo));
            Assert.Equal(SagaEstado.Concluida, saga.Estado);
        }

        [Fact(DisplayName = "Compensação exige motivo")]
        [Trait("Saga", "Domain")]
        public void Compensar_SemMotivo_DeveFalhar()
        {
            var saga = Criar();

            Assert.Throws<DomainException>(() => saga.Compensar("Timeout", string.Empty, Agora, Prazo));
        }

        [Fact(DisplayName = "Prazo vencido marca a Saga como expirada")]
        [Trait("Saga", "Domain")]
        public void Expirou_DeveConsiderarOPrazo()
        {
            var saga = Criar();

            Assert.False(saga.Expirou(Agora));
            Assert.True(saga.Expirou(Agora.Add(Prazo)));
            Assert.True(saga.Expirou(Agora.AddHours(2)));
        }

        [Fact(DisplayName = "Saga encerrada nunca expira")]
        [Trait("Saga", "Domain")]
        public void Expirou_SagaEncerrada_DeveSerFalso()
        {
            var saga = Criar();
            saga.Compensar("OrcamentoFalhou", "Billing indisponivel", Agora, Prazo);

            Assert.False(saga.Expirou(Agora.AddYears(1)));
        }

        [Fact(DisplayName = "Cada transição entra no histórico com gatilho e motivo")]
        [Trait("Saga", "Domain")]
        public void Transicoes_DevemRegistrarGatilhoEMotivo()
        {
            var saga = Criar();
            saga.AoCriarOrcamento(Guid.NewGuid(), Agora, Prazo);
            saga.Compensar("OrcamentoRecusado", "Cliente recusou", Agora, Prazo);

            var ultima = saga.Transicoes.Last();

            Assert.Equal("OrcamentoRecusado", ultima.Gatilho);
            Assert.Equal("Cliente recusou", ultima.Motivo);
            Assert.Equal(SagaEstado.AguardandoAprovacao, ultima.EstadoAnterior);
            Assert.Equal(SagaEstado.Compensada, ultima.EstadoNovo);
            Assert.Equal(Agora, ultima.OcorridaEm);
        }

        [Fact(DisplayName = "Pagamento aprovado sem aprovação prévia do orçamento é aceito")]
        [Trait("Saga", "Domain")]
        public void AoAprovarPagamento_ForaDeOrdemAceitavel_DeveTransitar()
        {
            var saga = Criar();
            saga.AoCriarOrcamento(Guid.NewGuid(), Agora, Prazo);

            Assert.True(saga.AoAprovarPagamento(Guid.NewGuid(), 750m, Agora, Prazo));

            Assert.Equal(SagaEstado.AguardandoExecucao, saga.Estado);
            Assert.Equal(750m, saga.ValorAprovado);
        }
    }
}
