using FluentValidation;
using wrench.auto.repair.core.Messages;
using wrench.auto.repair.ordem.servico.application.Queries.ViewModels;

namespace wrench.auto.repair.ordem.servico.application.Queries
{
    public class ObterOrdemServicoIdQuery(Guid id) : Command<OrdemServicoViewModel>
    {
        public Guid Id { get; private set; } = id;
        public override bool EhValido()
        {
            ValidationResult = new ObterOrdemServicoIdQueryValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class ObterOrdemServicoIdQueryValidator : AbstractValidator<ObterOrdemServicoIdQuery>
    {
        public ObterOrdemServicoIdQueryValidator()
        {
            RuleFor(c => c.Id)
                .NotEmpty()
                .WithMessage("O identificador da ordem de serviço deve ser informado");
        }
    }
}
