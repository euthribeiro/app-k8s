using System.Collections.Concurrent;
using wrench.auto.repair.core.Errors;
using wrench.auto.repair.core.Services;
using wrench.auto.repair.ordem.servico.domain.Saga.Mensageria;

namespace wrench.web.api.integration.tests.Base
{
    /// <summary>
    /// Substitui o envio de e-mail pelo SES. O cliente real resolve credenciais da AWS pela cadeia
    /// padrão, que não existe no runner de CI, e a exceção derrubava com 500 todos os endpoints de
    /// Ordem de Serviço que notificam o cliente.
    /// </summary>
    public sealed class EmailServiceFake : IEmailService
    {
        private readonly ConcurrentQueue<EmailMessage> _enviados = new();

        public IReadOnlyCollection<EmailMessage> Enviados => _enviados;

        public Task<Result<string>> EnviarAsync(EmailMessage mensagem, CancellationToken cancellationToken = default)
        {
            _enviados.Enqueue(mensagem);

            return Task.FromResult(Result<string>.Ok(Guid.NewGuid().ToString()));
        }
    }

    /// <summary>
    /// Substitui a publicação no SNS. Mantém o outbox transacional sendo exercitado de ponta a
    /// ponta sem que o host de teste precise falar com a AWS.
    /// </summary>
    public sealed class PublicadorSagaFake : IPublicadorSaga
    {
        private readonly ConcurrentQueue<MensagemSaga> _publicadas = new();

        public IReadOnlyCollection<MensagemSaga> Publicadas => _publicadas;

        public Task PublicarAsync(MensagemSaga mensagem, CancellationToken cancellationToken)
        {
            _publicadas.Enqueue(mensagem);

            return Task.CompletedTask;
        }
    }
}
