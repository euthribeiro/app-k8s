using FluentValidation;
using wrench.auto.repair.core.Messages;

namespace wrench.auto.repair.ordem.servico.application.UseCases.OrcamentoUseCase
{
    public class AprovaOrcamentoCommand(Guid ordemServicoId) : Command
    {
        public Guid OrdemServicoId { get; set; } = ordemServicoId;

        public override bool EhValido()
        {
            ValidationResult = new AprovaOrcamentoCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }

        public class AprovaOrcamentoCommandValidator : AbstractValidator<AprovaOrcamentoCommand>
        {
            public AprovaOrcamentoCommandValidator()
            {
                RuleFor(c => c.OrdemServicoId)
                    .NotEmpty()
                    .WithMessage("Ordem de serviço não informada.");

            }
        }
    }
}
