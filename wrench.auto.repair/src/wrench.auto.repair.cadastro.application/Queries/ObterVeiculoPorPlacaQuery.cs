using FluentValidation;
using wrench.auto.repair.cadastro.application.Queries.ViewModels;
using wrench.auto.repair.core.Messages;

namespace wrench.auto.repair.cadastro.application.Queries
{
    public class ObterVeiculoPorPlacaQuery : Command<VeiculoViewModel>
    {
        public ObterVeiculoPorPlacaQuery(string placa)
        {
            Placa = placa;
        }

        public string Placa { get; private set; }

        public override bool EhValido()
        {
            ValidationResult = new ObterVeiculoPorPlacaQueryValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class ObterVeiculoPorPlacaQueryValidator : AbstractValidator<ObterVeiculoPorPlacaQuery>
    {
        public static string PlacaVaziaError =>
            "A placa do veículo não pode ser vazio.";

        public static string PlacaInvalidaError =>
        "A placa do veículo informado não é válido.";

        public ObterVeiculoPorPlacaQueryValidator()
        {
            RuleFor(v => v.Placa)
                .NotEmpty()
                .WithMessage(PlacaVaziaError)
                .Matches("^[A-Z]{3}-?[0-9][0-9A-Z][0-9]{2}$")
                .WithMessage(PlacaInvalidaError);
        }
    }
}
