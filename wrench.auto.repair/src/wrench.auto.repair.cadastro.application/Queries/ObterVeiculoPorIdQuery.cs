using FluentValidation;
using wrench.auto.repair.cadastro.application.Queries.ViewModels;
using wrench.auto.repair.core.Messages;

namespace wrench.auto.repair.cadastro.application.Queries
{
    public class ObterVeiculoPorIdQuery(Guid veiculoId) : Command<VeiculoViewModel>
    {
        public Guid VeiculoId { get; private set; } = veiculoId;

        public override bool EhValido()
        {
            ValidationResult = new ObterVeiculoPorIdQueryValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class ObterVeiculoPorIdQueryValidator : AbstractValidator<ObterVeiculoPorIdQuery>
    {
        public static string VeiculoIdVazioError =>
            "O identificador do veículo não pode ser vazio.";

        public ObterVeiculoPorIdQueryValidator()
        {
            RuleFor(v => v.VeiculoId)
                .NotEmpty()
                .WithMessage(VeiculoIdVazioError);
        }
    }
}
