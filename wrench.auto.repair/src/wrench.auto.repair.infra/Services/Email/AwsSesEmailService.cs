using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Options;
using wrench.auto.repair.core.Errors;
using wrench.auto.repair.core.Services;
using wrench.auto.repair.infra.Options;

namespace wrench.auto.repair.infra.Services.Email
{
    public class AwsSesEmailService : IEmailService
    {
        private readonly IAmazonSimpleEmailService _amazonSimpleEmailService;
        private readonly EmailOptions _options;
        private readonly string ASPNETCORE_ENVIRONMENT;

        public AwsSesEmailService(
            IAmazonSimpleEmailService amazonSimpleEmailService,
            IOptions<EmailOptions> options)
        {
            _amazonSimpleEmailService = amazonSimpleEmailService;
            _options = options.Value;
            ASPNETCORE_ENVIRONMENT = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        }

        public async Task<Result<string>> EnviarAsync(EmailMessage mensagem, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(mensagem);

            if (!mensagem.EhValido())
            {
                throw new ArgumentException("A mensagem de e-mail é inválida.", nameof(mensagem));
            }

            if(!(ASPNETCORE_ENVIRONMENT?.ToUpper() == "PRODUCTION"))
            {
                return Result<string>.Ok("Ambiente de teste, e-mail não enviado.");
            }

            var sendEmailRequest = CriarEmailRequest(mensagem);
            var response = await _amazonSimpleEmailService.SendEmailAsync(sendEmailRequest, cancellationToken);

            if (response == null || response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                return Result<string>.Unexpected("Falha ao enviar o e-mail via AWS SES.");
            }

            return Result<string>.Created(response.MessageId);
        }

        private SendEmailRequest CriarEmailRequest(EmailMessage mensagem)
        {
            var source = string.IsNullOrWhiteSpace(mensagem.From)
                ? _options.FromAddress
                : mensagem.From;

            if (string.IsNullOrWhiteSpace(source))
            {
                throw new InvalidOperationException("O endereço do remetente do SES não foi configurado.");
            }

            return new SendEmailRequest
            {
                Source = source,
                Destination = new Destination
                {
                    ToAddresses = mensagem.To.ToList()
                },
                Message = new Message
                {
                    Subject = new Content { Data = mensagem.Subject, Charset = "UTF-8" },
                    Body = mensagem.IsHtml
                        ? new Body { Html = new Content { Charset = "UTF-8", Data = mensagem.Body } }
                        : new Body { Text = new Content { Charset = "UTF-8", Data = mensagem.Body } }
                }
            };
        }
    }
}
