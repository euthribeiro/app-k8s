using FluentValidation;
using wrench.auto.repair.core.Messages;
using wrench.auto.repair.ordem.servico.domain.Entities;

namespace wrench.auto.repair.ordem.servico.application.UseCases.OrdemServicoUseCase
{
    public class CriarOrdemServicoCommand : Command<Guid>
    {
        public CriarOrdemServicoCommand(Guid clienteId, Guid veiculoId, string descricao)
        {
            ClienteId = clienteId;
            VeiculoId = veiculoId;
            Descricao = descricao;
        }

        public Guid ClienteId { get; private set; }
        public Guid VeiculoId { get; private set; }
        public string Descricao { get; private set; }

        public override bool EhValido()
        {
            ValidationResult = new CriarOrdemServicoCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }

        public static implicit operator OrdemServico(CriarOrdemServicoCommand command)
        {
            return new OrdemServico(
                command.ClienteId,
                command.VeiculoId,
                command.Descricao,
                domain.Enums.OrdemServicoStatus.Recebida,
                DateTime.UtcNow
            );
        }

        public class CriarOrdemServicoCommandValidator : AbstractValidator<CriarOrdemServicoCommand>
        {
            public CriarOrdemServicoCommandValidator()
            {
                RuleFor(c => c.ClienteId)
                    .NotEmpty()
                    .WithMessage("Cliente não informado.");                    

                RuleFor(c => c.VeiculoId)
                 .NotEmpty()
                 .WithMessage("Veículo não informado.");

                RuleFor(c => c.Descricao)
                    .NotEmpty()
                    .WithMessage("Descrição não informada.");
            }
        }
    }
}
