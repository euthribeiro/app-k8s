namespace wrench.auto.repair.core.Services
{
    public sealed class EmailMessage
    {
        public required IReadOnlyCollection<string> To { get; init; }

        public required string Subject { get; init; }

        public required string Body { get; init; }

        public bool IsHtml { get; init; } = true;

        public string? From { get; init; }

        public bool EhValido()
        {
            if (To == null || !To.Any())
                throw new ArgumentException("O campo 'To' não pode ser nulo ou vazio.", nameof(To));
            if (string.IsNullOrWhiteSpace(Subject))
                throw new ArgumentException("O campo 'Subject' não pode ser nulo ou vazio.", nameof(Subject));
            if (string.IsNullOrWhiteSpace(Body))
                throw new ArgumentException("O campo 'Body' não pode ser nulo ou vazio.", nameof(Body));
            return true;
        }
    }
}
