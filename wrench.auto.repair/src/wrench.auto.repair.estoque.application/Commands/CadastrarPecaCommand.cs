using FluentValidation;
using wrench.auto.repair.core.Messages;

namespace wrench.auto.repair.estoque.application.Commands
{
    public class CadastrarPecaCommand(string nome, string descricao, double valor, int quantidade, bool ativo) : Command<Guid>
    {
        public string Nome { get; private set; } = nome;
        public string Descricao { get; private set; } = descricao;
        public double Valor { get; private set; } = valor;
        public int Quantidade { get; private set; } = quantidade;
        public bool Ativo { get; private set; } = ativo;

        public override bool EhValido()
        {
            ValidationResult = new CadastrarPecaCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class CadastrarPecaCommandValidator : AbstractValidator<CadastrarPecaCommand>
    {
        private static readonly int VALOR_MINIMO = 0;

        public static string NomeVazioError => "O nome não pode ser vazio";
        public static string DescricaoVazioError => "A descrição não pode ser vazia";
        public static string ValorMinimoError => $"O valor deve ser maior que {VALOR_MINIMO}";
        public static string QuantidadeMinimaError => $"A quantidade deve ser maior que {VALOR_MINIMO}";

        public CadastrarPecaCommandValidator()
        {
            RuleFor(c => c.Nome)
                .NotEmpty()
                .WithMessage(NomeVazioError);

            RuleFor(c => c.Descricao)
            .NotEmpty()
            .WithMessage(DescricaoVazioError);

            RuleFor(c => c.Valor)
              .GreaterThanOrEqualTo(VALOR_MINIMO)
              .WithMessage(ValorMinimoError);

            RuleFor(c => c.Quantidade)
              .GreaterThanOrEqualTo(0)
              .WithMessage(QuantidadeMinimaError);
        }
    }
}
