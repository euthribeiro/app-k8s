using FluentValidation;
using wrench.auto.repair.core.Messages.CommonMessages.IntegratedQueries.Dtos;

namespace wrench.auto.repair.core.Messages.CommonMessages.IntegratedQueries
{
    public class ObterPecasPorIdsCommand(IEnumerable<Guid> pecaIds) : IntegratedQuery<IEnumerable<PecaDto>>
    {
        public IEnumerable<Guid> PecasIds { get; private set; } = pecaIds;

        public override bool EhValido()
        {
            ValidationResult = new ObterPecasPorIdsQueryValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class ObterPecasPorIdsQueryValidator : AbstractValidator<ObterPecasPorIdsCommand>
    {
        public static string PecaIdVazio =>
                "Um dos identificador das peças informadas não é válido";

        public static string MaximoPecasPermitadas =>
            "O número máximo de peças permitidas é 100";

        public static string ListaPecasNula =>
            "A lista de peças não pode ser nula.";

        public static string MinimoPecas =>
            "Informe ao menos uma peça.";

        public ObterPecasPorIdsQueryValidator()
        {
            RuleFor(x => x.PecasIds)
                .NotNull()
                .WithMessage(ListaPecasNula)
                .NotEmpty()
                .WithMessage(MinimoPecas);

            RuleFor(x => x.PecasIds)
              .Must(x => x.Count() <= 100)
              .WithMessage(MaximoPecasPermitadas);

            RuleForEach(x => x.PecasIds)
                .NotEmpty()
                .WithMessage(PecaIdVazio);
        }
    }
}
