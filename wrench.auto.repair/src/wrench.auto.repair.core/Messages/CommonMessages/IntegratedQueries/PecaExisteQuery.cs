using FluentValidation;

namespace wrench.auto.repair.core.Messages.CommonMessages.IntegratedQueries
{
    public class PecaExisteQuery(Guid pecaId) : IntegratedQuery
    {
        public Guid PecaId { get; private set; } = pecaId;

        public override bool EhValido()
        {
            ValidationResult = new PecaExisteQueryValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class PecaExisteQueryValidator : AbstractValidator<PecaExisteQuery>
    {
        public static string PecaIdVazio =>
            "O identificador da peça não pode ser vazio";

        public PecaExisteQueryValidator()
        {
            RuleFor(c => c.PecaId)
                .NotEmpty()
                .WithMessage(PecaIdVazio);
        }
    }
}
