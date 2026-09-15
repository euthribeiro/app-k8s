using FluentValidation;
using wrench.auto.repair.core.Messages;

namespace wrench.auto.repair.ordem.servico.application.UseCases.OrdemServicoUseCase
{
    public class EntregarServicoCommand(Guid ordemServicoId) : Command
    {
        public Guid OrdemServicoId { get; private set; } = ordemServicoId;

        public override bool EhValido()
        {
            ValidationResult = new EntregarServicoCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }

        public class EntregarServicoCommandValidator : AbstractValidator<EntregarServicoCommand>
        {
            public EntregarServicoCommandValidator()
            {
                RuleFor(c => c.OrdemServicoId)
                    .NotEqual(Guid.Empty)
                    .WithMessage("ID da ordem de serviço não informado.");

            }
        }
    }
}