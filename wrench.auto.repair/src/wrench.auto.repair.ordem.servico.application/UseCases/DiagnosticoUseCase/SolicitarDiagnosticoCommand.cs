using FluentValidation;
using wrench.auto.repair.core.Messages;

namespace wrench.auto.repair.ordem.servico.application.UseCases.DiagnosticoUseCase
{
    public class SolicitarDiagnosticoCommand : Command
    {
        public Guid OrdemServicoId { get; set; }

        public SolicitarDiagnosticoCommand(Guid ordemServicoId)
        {
            OrdemServicoId = ordemServicoId;
        }

        public override bool EhValido()
        {
            ValidationResult = new CriarDiagnosticoCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }

        public class CriarDiagnosticoCommandValidator : AbstractValidator<SolicitarDiagnosticoCommand>
        {
            public CriarDiagnosticoCommandValidator()
            {
                RuleFor(c => c.OrdemServicoId)
                    .NotEmpty()
                    .WithMessage("Ordem de serviço não informada.");

            }
        }
    }
}