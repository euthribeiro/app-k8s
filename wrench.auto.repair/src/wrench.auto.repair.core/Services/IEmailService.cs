using wrench.auto.repair.core.Errors;

namespace wrench.auto.repair.core.Services
{
    public interface IEmailService
    {
        Task<Result<string>> EnviarAsync(EmailMessage mensagem, CancellationToken cancellationToken = default);
    }
}
