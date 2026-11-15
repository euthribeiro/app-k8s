using FluentValidation;
using wrench.auto.repair.core.Messages;

namespace wrench.auto.repair.ordem.servico.application.UseCases.OrdemServicoUseCase
{
    public class RetornarParaDiagnosticoCommand : Command
    {
        public RetornarParaDiagnosticoCommand(Guid ordemServicoId, string motivo)
        {
            OrdemServicoId = ordemServicoId;
            Motivo = motivo;
        }

        public Guid OrdemServicoId { get; private set; }
        public string Motivo { get; private set; }

        public override bool EhValido()
        {
            ValidationResult = new RetornarParaDiagnosticoCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }

        public class RetornarParaDiagnosticoCommandValidator : AbstractValidator<RetornarParaDiagnosticoCommand>
        {
            public RetornarParaDiagnosticoCommandValidator()
            {
                RuleFor(c => c.OrdemServicoId)
                    .NotEqual(Guid.Empty)
                    .WithMessage("ID da ordem de serviço não informado.");

                RuleFor(c => c.Motivo)
                    .NotEmpty()
                    .WithMessage("O motivo do retorno para diagnóstico deve ser informado.");
            }
        }
    }
}
