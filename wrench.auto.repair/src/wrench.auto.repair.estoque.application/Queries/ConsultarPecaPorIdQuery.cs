using FluentValidation;
using wrench.auto.repair.core.Messages;
using wrench.auto.repair.estoque.application.Queries.ViewModels;

namespace wrench.auto.repair.estoque.application.Queries
{
    public class ConsultarPecaPorIdQuery(Guid pecaId) : Command<PecaViewModel>
    {
        public Guid PecaId { get; private set; } = pecaId;

        public override bool EhValido()
        {
            ValidationResult = new ConsultarPecaPorIdQueryValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class ConsultarPecaPorIdQueryValidator : AbstractValidator<ConsultarPecaPorIdQuery>
    {
        public static string PecaIdVazioError =>
            "O identificador da peça precisa ser informado";

        public ConsultarPecaPorIdQueryValidator()
        {
            RuleFor(c => c.PecaId)
                .NotEmpty()
                .WithMessage(PecaIdVazioError);
        }
    }
}
