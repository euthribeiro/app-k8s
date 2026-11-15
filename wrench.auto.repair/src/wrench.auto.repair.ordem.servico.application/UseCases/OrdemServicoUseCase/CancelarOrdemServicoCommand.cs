using FluentValidation;
using wrench.auto.repair.core.Messages;

namespace wrench.auto.repair.ordem.servico.application.UseCases.OrdemServicoUseCase
{
    public class CancelarOrdemServicoCommand : Command
    {
        public CancelarOrdemServicoCommand(Guid ordemServicoId, string motivo)
        {
            OrdemServicoId = ordemServicoId;
            Motivo = motivo;
        }

        public Guid OrdemServicoId { get; private set; }
        public string Motivo { get; private set; }

        public override bool EhValido()
        {
            ValidationResult = new CancelarOrdemServicoCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }

        public class CancelarOrdemServicoCommandValidator : AbstractValidator<CancelarOrdemServicoCommand>
        {
            public CancelarOrdemServicoCommandValidator()
            {
                RuleFor(c => c.OrdemServicoId)
                    .NotEqual(Guid.Empty)
                    .WithMessage("ID da ordem de serviço não informado.");

                RuleFor(c => c.Motivo)
                    .NotEmpty()
                    .WithMessage("O motivo do cancelamento deve ser informado.");
            }
        }
    }
}
