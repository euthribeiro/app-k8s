using FluentValidation;
using wrench.auto.repair.core.Messages;

namespace wrench.auto.repair.estoque.application.Commands
{
    public class InativarPecaCommand(Guid pecaId) : Command
    {
        public Guid PecaId { get; private set; } = pecaId;

        public override bool EhValido()
        {
            ValidationResult = new InativarPecaCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class InativarPecaCommandValidator : AbstractValidator<InativarPecaCommand>
    {
        public static string PecaIdVazioError =>
            "O identificador da peça precisa ser informado";

        public InativarPecaCommandValidator()
        {
            RuleFor(c => c.PecaId)
                .NotEmpty()
                .WithMessage(PecaIdVazioError);
        }
    }
}
