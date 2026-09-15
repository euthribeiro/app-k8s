using FluentValidation;
using wrench.auto.repair.core.Messages;

namespace wrench.auto.repair.autenticacao.application.Commands
{
    public class ResetarSenhaUsuarioCommand(Guid usuarioId) : Command
    {
        public Guid UsuarioId { get; private set; } = usuarioId;

        public override bool EhValido()
        {
            ValidationResult = new ResetarSenhaUsuarioCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class ResetarSenhaUsuarioCommandValidator : AbstractValidator<ResetarSenhaUsuarioCommand>
    {
        public static string UsuarioIdVazio => "O identificador do usuário precisa ser informado.";

        public ResetarSenhaUsuarioCommandValidator()
        {
            RuleFor(c => c.UsuarioId)
                .NotEmpty()
                .WithMessage(UsuarioIdVazio);
        }
    }
}
