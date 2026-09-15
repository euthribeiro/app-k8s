using FluentValidation;
using wrench.auto.repair.core.Messages;

namespace wrench.auto.repair.ordem.servico.application.UseCases.OrdemServicoUseCase
{
    public class FinalizarOrdemServicoCommand : Command
    {
        public FinalizarOrdemServicoCommand(Guid ordemServicoId)
        {
            OrdemServicoId = ordemServicoId;
        }

        public Guid OrdemServicoId { get; private set; }

        public override bool EhValido()
        {
            ValidationResult = new FinalizarOrdemServicoCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }

        public class FinalizarOrdemServicoCommandValidator : AbstractValidator<FinalizarOrdemServicoCommand>
        {
            public FinalizarOrdemServicoCommandValidator()
            {
                RuleFor(c => c.OrdemServicoId)
                    .NotEqual(Guid.Empty)
                    .WithMessage("ID da ordem de serviço não informado.");

            }
        }
    }
}