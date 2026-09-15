using FluentValidation;
using wrench.auto.repair.core.Messages;

namespace wrench.auto.repair.autenticacao.application.Commands
{
    public class AtivarUsuarioCommand(Guid usuarioId) : Command
    {
        public Guid UsuarioId { get; private set; } = usuarioId;

        public override bool EhValido()
        {
            ValidationResult = new AtivarUsuarioCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class AtivarUsuarioCommandValidator : AbstractValidator<AtivarUsuarioCommand>
    {
        public static string UsuarioIdVazio => "O identificador do usuário precisa ser informado.";

        public AtivarUsuarioCommandValidator()
        {
            RuleFor(c => c.UsuarioId)
                .NotEmpty()
                .WithMessage(UsuarioIdVazio);
        }
    }
}
