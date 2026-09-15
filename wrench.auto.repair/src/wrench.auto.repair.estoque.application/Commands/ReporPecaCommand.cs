using FluentValidation;
using wrench.auto.repair.core.Messages;

namespace wrench.auto.repair.estoque.application.Commands
{
    public class ReporPecaCommand(Guid pecaId, int quantidade) : Command
    {
        public Guid PecaId { get; private set; } = pecaId;
        public int Quantidade { get; private set; } = quantidade;

        public override bool EhValido()
        {
            ValidationResult = new ReporPecaCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class ReporPecaCommandValidator : AbstractValidator<ReporPecaCommand>
    {
        private static readonly int QUANTIDADE_MINIMA = 1;

        public static string PecaIdVazioError =>
            "O identificador da peça não pode ser vazio";

        public static string QuantidadeMinimaError =>
            $"A quantidade mínima para repor o estoque é de {QUANTIDADE_MINIMA}";

        public ReporPecaCommandValidator()
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
