using FluentValidation;
using wrench.auto.repair.core.Messages;
using wrench.auto.repair.ordem.servico.application.Queries.ViewModels;

namespace wrench.auto.repair.ordem.servico.application.Queries
{
    public class ObterHistoricoOrdemServicoQuery(Guid id) : Command<List<OrdemServicoHistoricoViewModel>>
    {
        public Guid Id { get; private set; } = id;

        public override bool EhValido()
        {
            ValidationResult = new ObterHistoricoOrdemServicoQueryValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class ObterHistoricoOrdemServicoQueryValidator : AbstractValidator<ObterHistoricoOrdemServicoQuery>
    {
        public ObterHistoricoOrdemServicoQueryValidator()
        {
            RuleFor(c => c.Id)
                .NotEmpty()
                .WithMessage("O identificador da ordem de serviço deve ser informado");
        }
    }
}
