using FluentValidation;
using wrench.auto.repair.core.Messages;

namespace wrench.auto.repair.estoque.application.Commands
{
    public class AtivarPecaCommand(Guid pecaId) : Command
    {
        public Guid PecaId { get; private set; } = pecaId;

        public override bool EhValido()
        {
            ValidationResult = new AtivarPecaCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class AtivarPecaCommandValidator : AbstractValidator<AtivarPecaCommand>
    {
        public static string PecaIdVazioError =>
            "O identificador da peça precisa ser informado";

        public AtivarPecaCommandValidator()
        {
            RuleFor(c => c.PecaId)
                .NotEmpty()
                .WithMessage(PecaIdVazioError);
        }
    }
}
