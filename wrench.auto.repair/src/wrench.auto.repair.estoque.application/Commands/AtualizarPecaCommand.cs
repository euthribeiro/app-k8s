using FluentValidation;
using wrench.auto.repair.core.Messages;

namespace wrench.auto.repair.estoque.application.Commands
{
    public class AtualizarPecaCommand(Guid pecaId, string nome, string descricao, double valor, bool ativo) : Command
    {
        public Guid PecaId { get; private set; } = pecaId;
        public string Nome { get; private set; } = nome;
        public string Descricao { get; private set; } = descricao;
        public double Valor { get; private set; } = valor;
        public bool Ativo { get; private set; } = ativo;

        public override bool EhValido()
        {
            ValidationResult = new AtualizarPecaCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class AtualizarPecaCommandValidator : AbstractValidator<AtualizarPecaCommand>
    {
        private static readonly int VALOR_MINIMO = 0;

        public static string PecaIdVazioError => "O identificador da peça não pode ser nulo.";
        public static string NomeVazioError => "O nome não pode ser vazio";
        public static string DescricaoVazioError => "A descrição não pode ser vazia";
        public static string ValorMinimoError => $"O valor deve ser maior que {VALOR_MINIMO}";

        public AtualizarPecaCommandValidator()
        {
            RuleFor(c => c.PecaId)
               .NotEmpty()
               .WithMessage(PecaIdVazioError);

            RuleFor(c => c.Nome)
                .NotEmpty()
                .WithMessage(NomeVazioError);

            RuleFor(c => c.Descricao)
            .NotEmpty()
            .WithMessage(DescricaoVazioError);

            RuleFor(c => c.Valor)
              .GreaterThanOrEqualTo(VALOR_MINIMO)
              .WithMessage(ValorMinimoError);
        }
    }
}
