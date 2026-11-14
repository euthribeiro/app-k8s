using wrench.auto.repair.core.DomainObjects;

namespace wrench.auto.repair.ordem.servico.domain.Saga
{
    /// <summary>
    /// Máquina de estados da Saga de uma ordem de serviço. Guarda em que etapa do fluxo
    /// distribuído a OS está, até quando essa etapa pode esperar e qual compensação aplicar
    /// quando ela falha ou expira. Transições inválidas ou repetidas não mudam o estado e
    /// devolvem <c>false</c>, o que torna o consumo de mensagens duplicadas inofensivo.
    /// </summary>
    public class SagaOrdemServico : Entity, IAggregateRoot
    {
        public Guid OrdemServicoId { get; private set; }
        public string CorrelationId { get; private set; }
        public SagaEstado Estado { get; private set; }
        public SagaEtapa Etapa { get; private set; }
        public DateTime? PrazoEtapa { get; private set; }
        public string? MotivoDesfecho { get; private set; }
        public Guid? OrcamentoId { get; private set; }
        public Guid? PagamentoId { get; private set; }
        public decimal? ValorAprovado { get; private set; }
        public DateTime DataCriacao { get; private set; }
        public DateTime DataAtualizacao { get; private set; }
        public DateTime? DataDesfecho { get; private set; }

        private readonly List<SagaTransicao> _transicoes = [];
        public IReadOnlyCollection<SagaTransicao> Transicoes => _transicoes.AsReadOnly();

        private static readonly SagaEstado[] EstadosTerminais =
        [
            SagaEstado.Concluida,
            SagaEstado.Compensada,
            SagaEstado.Falha
        ];

        private SagaOrdemServico()
        {
            CorrelationId = string.Empty;
        }

        public SagaOrdemServico(Guid ordemServicoId, string correlationId, DateTime agora, TimeSpan prazoOrcamento)
        {
            Validacoes.ValidarSeVazio(ordemServicoId, "O identificador da ordem de serviço não pode ser vazio");
            Validacoes.ValidarSeVazio(correlationId, "A correlação da Saga não pode ser vazia");

            OrdemServicoId = ordemServicoId;
            CorrelationId = correlationId;
            Estado = SagaEstado.AguardandoOrcamento;
            Etapa = SagaEtapa.Orcamento;
            PrazoEtapa = agora.Add(prazoOrcamento);
            DataCriacao = agora;
            DataAtualizacao = agora;

            Registrar(null, SagaEstado.AguardandoOrcamento, SagaEtapa.Orcamento, "GerarOrcamento", null, agora);
        }

        public bool Encerrada => EstadosTerminais.Contains(Estado);

        public bool Expirou(DateTime agora) => !Encerrada && PrazoEtapa.HasValue && PrazoEtapa.Value <= agora;

        /// <summary>O orçamento foi criado no Billing; a Saga passa a esperar a decisão do cliente.</summary>
        public bool AoCriarOrcamento(Guid orcamentoId, DateTime agora, TimeSpan prazoAprovacao)
        {
            if (Estado != SagaEstado.AguardandoOrcamento) return false;

            OrcamentoId = orcamentoId;

            Transitar(SagaEstado.AguardandoAprovacao, SagaEtapa.Aprovacao, "OrcamentoCriado", null, agora, agora.Add(prazoAprovacao));

            return true;
        }

        /// <summary>O cliente aprovou o orçamento; a Saga passa a esperar a confirmação do pagamento.</summary>
        public bool AoAprovarOrcamento(Guid orcamentoId, decimal valorAprovado, DateTime agora, TimeSpan prazoPagamento)
        {
            if (Estado is not (SagaEstado.AguardandoAprovacao or SagaEstado.AguardandoOrcamento)) return false;

            OrcamentoId = orcamentoId;
            ValorAprovado = valorAprovado;

            Transitar(SagaEstado.AguardandoPagamento, SagaEtapa.Pagamento, "OrcamentoAprovado", null, agora, agora.Add(prazoPagamento));

            return true;
        }

        /// <summary>O pagamento foi confirmado; a Saga passa a esperar o desfecho da execução.</summary>
        public bool AoAprovarPagamento(Guid pagamentoId, decimal valor, DateTime agora, TimeSpan prazoExecucao)
        {
            if (Estado is not (SagaEstado.AguardandoPagamento or SagaEstado.AguardandoAprovacao)) return false;

            PagamentoId = pagamentoId;
            ValorAprovado = valor;

            Transitar(SagaEstado.AguardandoExecucao, SagaEtapa.Execucao, "PagamentoAprovado", null, agora, agora.Add(prazoExecucao));

            return true;
        }

        /// <summary>O reparo terminou; a Saga chega ao fim pelo caminho feliz.</summary>
        public bool AoConcluirExecucao(DateTime agora)
        {
            if (Estado != SagaEstado.AguardandoExecucao) return false;

            Transitar(SagaEstado.Concluida, SagaEtapa.Nenhuma, "ExecucaoConcluida", null, agora, null);
            DataDesfecho = agora;

            return true;
        }

        /// <summary>
        /// Abre a compensação da etapa corrente. Quando a etapa é a execução o valor já foi
        /// cobrado: a Saga fica em Compensando até o estorno voltar do Billing. Nas demais
        /// etapas não há dinheiro a devolver e a compensação se encerra na mesma transição.
        /// </summary>
        public bool Compensar(string gatilho, string motivo, DateTime agora, TimeSpan prazoEstorno)
        {
            if (Encerrada) return false;

            Validacoes.ValidarSeVazio(motivo, "O motivo da compensação deve ser informado");

            var etapaCompensada = Etapa;
            MotivoDesfecho = motivo;

            if (etapaCompensada == SagaEtapa.Execucao && PagamentoId.HasValue)
            {
                Transitar(SagaEstado.Compensando, SagaEtapa.Estorno, gatilho, motivo, agora, agora.Add(prazoEstorno));
            }
            else
            {
                Transitar(SagaEstado.Compensada, etapaCompensada, gatilho, motivo, agora, null);
                DataDesfecho = agora;
            }

            return true;
        }

        /// <summary>O estorno foi concluído no Mercado Pago e fecha a compensação.</summary>
        public bool AoEstornarPagamento(DateTime agora)
        {
            if (Estado != SagaEstado.Compensando) return false;

            Transitar(SagaEstado.Compensada, SagaEtapa.Estorno, "PagamentoEstornado", MotivoDesfecho, agora, null);
            DataDesfecho = agora;

            return true;
        }

        /// <summary>O estorno não pôde ser concluído; a Saga para e exige tratamento manual.</summary>
        public bool AoFalharEstorno(string motivo, DateTime agora)
        {
            if (Estado != SagaEstado.Compensando) return false;

            MotivoDesfecho = motivo;

            Transitar(SagaEstado.Falha, SagaEtapa.Estorno, "EstornoFalhou", motivo, agora, null);
            DataDesfecho = agora;

            return true;
        }

        private void Transitar(
            SagaEstado estadoNovo,
            SagaEtapa etapaNova,
            string gatilho,
            string? motivo,
            DateTime agora,
            DateTime? prazo)
        {
            var estadoAnterior = Estado;

            Estado = estadoNovo;
            Etapa = etapaNova;
            PrazoEtapa = prazo;
            DataAtualizacao = agora;

            Registrar(estadoAnterior, estadoNovo, etapaNova, gatilho, motivo, agora);
        }

        private void Registrar(
            SagaEstado? estadoAnterior,
            SagaEstado estadoNovo,
            SagaEtapa etapa,
            string gatilho,
            string? motivo,
            DateTime agora)
        {
            _transicoes.Add(new SagaTransicao(estadoAnterior, estadoNovo, etapa, gatilho, motivo, agora));
        }
    }
}
