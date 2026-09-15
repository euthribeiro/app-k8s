using FluentValidation;
using wrench.auto.repair.core.Messages;

namespace wrench.auto.repair.ordem.servico.application.UseCases.OrcamentoUseCase
{
    public class RecusarOrcamentoCommand(Guid ordemServicoId, string motivoRecusa) : Command
    {
        public Guid OrdemServicoId { get; set; } = ordemServicoId;
        public string MotivoRecusa { get; private set; } = motivoRecusa;

        public override bool EhValido()
        {
            ValidationResult = new RecusarOrcamentoCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }

        public class RecusarOrcamentoCommandValidator : AbstractValidator<RecusarOrcamentoCommand>
        {
            public RecusarOrcamentoCommandValidator()
            {
                RuleFor(c => c.OrdemServicoId)
                    .NotEmpty()
                    .WithMessage("Ordem de serviço não informada.");

                RuleFor(c => c.MotivoRecusa)
                    .NotEmpty()
                    .WithMessage("O motivo da recusa deve ser informado.");
            }
        }
    }
}
