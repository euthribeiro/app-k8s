using FluentValidation;
using wrench.auto.repair.core.Messages;

namespace wrench.auto.repair.autenticacao.application.Commands
{
    public class InativarUsuarioCommand(Guid usuarioId) : Command
    {
        public Guid UsuarioId { get; private set; } = usuarioId;

        public override bool EhValido()
        {
            ValidationResult = new InativarUsuarioCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class InativarUsuarioCommandValidator : AbstractValidator<InativarUsuarioCommand>
    {
        public static string UsuarioIdVazio => "O identificador do usuário precisa ser informado.";

        public InativarUsuarioCommandValidator()
        {
            RuleFor(c => c.UsuarioId)
                .NotEmpty()
                .WithMessage(UsuarioIdVazio);
        }
    }
}
