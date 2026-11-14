namespace wrench.auto.repair.ordem.servico.infra.Saga.Mensageria
{
    /// <summary>Tópico SNS onde o OS Service publica os comandos da Saga.</summary>
    public class SnsOptions
    {
        public const string Secao = "Sns";

        public string TopicArn { get; set; } = string.Empty;

        /// <summary>Preenchido apenas em desenvolvimento (LocalStack).</summary>
        public string? ServiceUrl { get; set; }

        public string? Region { get; set; }
    }

    /// <summary>Fila SQS de onde o OS Service consome os eventos do Billing e da Execução.</summary>
    public class SqsOptions
    {
        public const string Secao = "Sqs";

        public string QueueUrl { get; set; } = string.Empty;

        public string? ServiceUrl { get; set; }

        public string? Region { get; set; }

        public int MensagensPorLeitura { get; set; } = 10;

        public int EsperaLongaSegundos { get; set; } = 20;

        /// <summary>Entregas a partir das quais a mensagem é contabilizada como destinada à DLQ.</summary>
        public int TentativasAteDlq { get; set; } = 5;
    }

    /// <summary>Ritmo do dispatcher que publica as mensagens pendentes do outbox.</summary>
    public class OutboxOptions
    {
        public const string Secao = "Outbox";

        public int IntervaloSegundos { get; set; } = 2;

        public int TamanhoLote { get; set; } = 25;
    }
}
