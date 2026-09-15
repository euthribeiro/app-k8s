using System.Net;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Options;
using Moq;
using wrench.auto.repair.core.Errors;
using wrench.auto.repair.core.Services;
using wrench.auto.repair.infra.Options;
using wrench.auto.repair.infra.Services.Email;

namespace wrench.auto.repair.infra.tests.Services.Email
{
    public class AwsSesEmailServiceTests
    {
        [Fact(DisplayName = "EnviarAsync deve enviar e-mail usando FromAddress configurado")]
        [Trait("Infra", "Email")]
        public async Task EnviarAsync_DeveUsarFromAddressConfiguradoQuandoMensagemNaoInformarRemetente()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "PRODUCTION");

            var amazonSimpleEmailServiceMock = new Mock<IAmazonSimpleEmailService>();
            SendEmailRequest? capturedRequest = null;

            amazonSimpleEmailServiceMock
                .Setup(service => service.SendEmailAsync(It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>()))
                .Callback<SendEmailRequest, CancellationToken>((request, _) => capturedRequest = request)
                .ReturnsAsync(new SendEmailResponse
                {
                    HttpStatusCode = HttpStatusCode.OK,
                    MessageId = "message-id-123"
                });

            var options = global::Microsoft.Extensions.Options.Options.Create(new EmailOptions
            {
                FromAddress = "brunocbarreto2012@gmail.com"
            });

            var sut = new AwsSesEmailService(amazonSimpleEmailServiceMock.Object, options);
            var mensagem = new EmailMessage
            {
                To = ["thiago_santos14@hotmail.com"],
                Subject = "Agendamento confirmado",
                Body = "<p>Sua ordem foi atualizada.</p>"
            };

            var result = await sut.EnviarAsync(mensagem);

            Assert.True(result.Sucesso);
            Assert.Equal(ResultadoStatusEnum.CRIADO, result.ResultadoStatus);
            Assert.Equal("message-id-123", result.Valor);
            Assert.NotNull(capturedRequest);
            Assert.Equal("brunocbarreto2012@gmail.com", capturedRequest!.Source);
            Assert.Equal(["thiago_santos14@hotmail.com"], capturedRequest.Destination.ToAddresses);
            Assert.Equal("Agendamento confirmado", capturedRequest.Message.Subject.Data);
            Assert.Equal("<p>Sua ordem foi atualizada.</p>", capturedRequest.Message.Body.Html!.Data);
            Assert.Null(capturedRequest.Message.Body.Text);
        }

        [Fact(DisplayName = "EnviarAsync deve usar o remetente informado na mensagem")]
        [Trait("Infra", "Email")]
        public async Task EnviarAsync_DeveUsarRemetenteDaMensagemQuandoInformado()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "PRODUCTION");

            var amazonSimpleEmailServiceMock = new Mock<IAmazonSimpleEmailService>();
            SendEmailRequest? capturedRequest = null;

            amazonSimpleEmailServiceMock
                .Setup(service => service.SendEmailAsync(It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>()))
                .Callback<SendEmailRequest, CancellationToken>((request, _) => capturedRequest = request)
                .ReturnsAsync(new SendEmailResponse
                {
                    HttpStatusCode = HttpStatusCode.OK,
                    MessageId = "message-id-456"
                });

            var options = global::Microsoft.Extensions.Options.Options.Create(new EmailOptions
            {
                FromAddress = "brunocbarreto2012@gmail.com"
            });

            var sut = new AwsSesEmailService(amazonSimpleEmailServiceMock.Object, options);
            var mensagem = new EmailMessage
            {
                To = ["thiago_santos14@hotmail.com"],
                Subject = "Atualização da ordem",
                Body = "Mensagem em texto puro",
                IsHtml = false,
                From = "brunocbarreto2012@gmail.com"
            };

            var result = await sut.EnviarAsync(mensagem);

            Assert.True(result.Sucesso);
            Assert.Equal(ResultadoStatusEnum.CRIADO, result.ResultadoStatus);
            Assert.Equal("message-id-456", result.Valor);
            Assert.NotNull(capturedRequest);
            Assert.Equal("brunocbarreto2012@gmail.com", capturedRequest!.Source);
            Assert.Equal(["thiago_santos14@hotmail.com"], capturedRequest.Destination.ToAddresses);
            Assert.Equal("Atualização da ordem", capturedRequest.Message.Subject.Data);
            Assert.Equal("Mensagem em texto puro", capturedRequest.Message.Body.Text!.Data);
            Assert.Null(capturedRequest.Message.Body.Html);
        }

        [Fact(DisplayName = "EnviarAsync deve retornar erro inesperado quando SES falhar")]
        [Trait("Infra", "Email")]
        public async Task EnviarAsync_DeveRetornarErroQuandoResponseNaoForOk()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "PRODUCTION");

            var amazonSimpleEmailServiceMock = new Mock<IAmazonSimpleEmailService>();

            amazonSimpleEmailServiceMock
                .Setup(service => service.SendEmailAsync(It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SendEmailResponse
                {
                    HttpStatusCode = HttpStatusCode.BadRequest,
                    MessageId = "message-id-789"
                });

            var options = global::Microsoft.Extensions.Options.Options.Create(new EmailOptions
            {
                FromAddress = "brunocbarreto2012@gmail.com"
            });

            var sut = new AwsSesEmailService(amazonSimpleEmailServiceMock.Object, options);
            var mensagem = new EmailMessage
            {
                To = ["thiago_santos14@hotmail.com"],
                Subject = "Falha no envio",
                Body = "Conteúdo da mensagem"
            };

            var result = await sut.EnviarAsync(mensagem);

            Assert.False(result.Sucesso);
            Assert.Equal(TipoErroEnum.INESPERADO, result.TipoErro);
            Assert.Contains("Falha ao enviar o e-mail via AWS SES.", result.Erros);
            Assert.Null(result.Valor);
        }
    }
}
