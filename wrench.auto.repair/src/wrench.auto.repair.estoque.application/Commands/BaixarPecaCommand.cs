using FluentValidation;
using wrench.auto.repair.core.Messages;

namespace wrench.auto.repair.estoque.application.Commands
{
    public class BaixarPecaCommand(Guid pecaId, int quantidade) : Command
    {
        public Guid PecaId { get; private set; } = pecaId;

        public int Quantidade { get; private set; } = quantidade;

        public override bool EhValido()
        {
            ValidationResult = new BaixarPecaCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class BaixarPecaCommandValidator : AbstractValidator<BaixarPecaCommand>
    {
        private static readonly int QUANTIDADE_MINIMA = 1;

        public static string PecaIdVazioError =>
            "O identificador da peça precisa ser informado";

        public static string QuantidadeMinimaError =>
            $"A quantidade mínima de peças deve ser {QUANTIDADE_MINIMA}";

        public BaixarPecaCommandValidator()
        {
            RuleFor(c => c.PecaId)
                .NotEmpty()
                .WithMessage(PecaIdVazioError);

            RuleFor(c => c.Quantidade)
                .GreaterThanOrEqualTo(QUANTIDADE_MINIMA)
                .WithMessage(QuantidadeMinimaError);
        }
    }
}
