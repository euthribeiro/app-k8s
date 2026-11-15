using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace wrench.auto.repair.ordem.servico.domain.Saga
{
    /// <summary>
    /// Telemetria da Saga: contadores e histograma publicados no meter <see cref="MeterName"/>
    /// e a <see cref="ActivitySource"/> que abre um span por mensagem consumida. O span carrega
    /// o <c>traceparent</c> recebido da mensagem, o que mantém o trace distribuído ligado do
    /// início ao fim da ordem de serviço. Os nomes aqui são os mesmos consultados pelo dashboard
    /// e pelos alertas em <c>infra-k8s/terraform/observability</c>.
    /// </summary>
    public static class SagaMetrics
    {
        public const string MeterName = "Wrench.Saga";
        public const string ActivitySourceName = "Wrench.Saga";

        public const string AtributoTipo = "tipo";
        public const string AtributoEtapa = "etapa";
        public const string AtributoEstado = "estado";

        private static readonly Meter Meter = new(MeterName);

        public static readonly ActivitySource Source = new(ActivitySourceName);

        private static readonly Counter<long> Iniciadas = Meter.CreateCounter<long>(
            "saga.iniciada", unit: "{saga}", description: "Sagas iniciadas apos o diagnostico da ordem de servico");

        private static readonly Counter<long> Concluidas = Meter.CreateCounter<long>(
            "saga.concluida", unit: "{saga}", description: "Sagas que chegaram ao fim pelo caminho feliz");

        private static readonly Counter<long> Compensacoes = Meter.CreateCounter<long>(
            "saga.compensacao", unit: "{saga}", description: "Sagas que entraram em compensacao, por etapa");

        private static readonly Counter<long> Timeouts = Meter.CreateCounter<long>(
            "saga.timeout", unit: "{saga}", description: "Etapas da Saga que estouraram o prazo sem resposta");

        private static readonly Counter<long> MensagensRecebidas = Meter.CreateCounter<long>(
            "saga.mensagem.recebida", unit: "{mensagem}", description: "Mensagens da Saga consumidas da fila");

        private static readonly Counter<long> MensagensFalhas = Meter.CreateCounter<long>(
            "saga.mensagem.falha", unit: "{mensagem}", description: "Mensagens cujo processamento terminou em excecao");

        private static readonly Counter<long> MensagensDlq = Meter.CreateCounter<long>(
            "saga.mensagem.dlq", unit: "{mensagem}", description: "Mensagens que esgotaram as tentativas e seguem para a DLQ");

        private static readonly Counter<long> MensagensPublicadas = Meter.CreateCounter<long>(
            "saga.mensagem.publicada", unit: "{mensagem}", description: "Mensagens do outbox publicadas no SNS");

        private static readonly Histogram<double> Duracao = Meter.CreateHistogram<double>(
            "saga.duracao", unit: "ms", description: "Tempo entre o inicio e o desfecho da Saga da ordem de servico");

        public static void SagaIniciada() => Iniciadas.Add(1);

        public static void SagaConcluida(double duracaoMs)
        {
            Concluidas.Add(1);
            Duracao.Record(duracaoMs);
        }

        public static void SagaCompensada(string etapa, double duracaoMs)
        {
            Compensacoes.Add(1, new KeyValuePair<string, object?>(AtributoEtapa, etapa));
            Duracao.Record(duracaoMs);
        }

        public static void TimeoutDaEtapa(string estado) =>
            Timeouts.Add(1, new KeyValuePair<string, object?>(AtributoEstado, estado));

        public static void MensagemRecebida(string tipo) =>
            MensagensRecebidas.Add(1, new KeyValuePair<string, object?>(AtributoTipo, tipo));

        public static void MensagemFalhou(string tipo) =>
            MensagensFalhas.Add(1, new KeyValuePair<string, object?>(AtributoTipo, tipo));

        public static void MensagemParaDlq(string tipo) =>
            MensagensDlq.Add(1, new KeyValuePair<string, object?>(AtributoTipo, tipo));

        public static void MensagemPublicada(string tipo) =>
            MensagensPublicadas.Add(1, new KeyValuePair<string, object?>(AtributoTipo, tipo));

        public static void RegistrarOutboxPendentes(Func<int> leitura) =>
            Meter.CreateObservableGauge(
                "saga.outbox.pendentes",
                () => leitura(),
                unit: "{mensagem}",
                description: "Mensagens ainda nao publicadas no outbox transacional");
    }
}
