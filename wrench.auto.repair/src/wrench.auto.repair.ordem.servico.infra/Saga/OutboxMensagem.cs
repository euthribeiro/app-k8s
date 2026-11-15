namespace wrench.auto.repair.ordem.servico.infra.Saga
{
    /// <summary>Linha do outbox transacional: a mensagem de saída ainda não publicada no SNS.</summary>
    public class OutboxMensagem
    {
        public Guid Id { get; set; }
        public Guid OrdemServicoId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
        public string? Traceparent { get; set; }
        public DateTime CriadaEm { get; set; }
        public DateTime? PublicadaEm { get; set; }
        public int Tentativas { get; set; }
        public string? UltimoErro { get; set; }
    }

    /// <summary>Linha do inbox: mensagem já consumida, usada para descartar entregas repetidas.</summary>
    public class InboxMensagem
    {
        public string MessageId { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public DateTime RecebidaEm { get; set; }
    }
}
