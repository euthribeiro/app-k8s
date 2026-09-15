using FluentValidation;
using wrench.auto.repair.cadastro.application.Queries.ViewModels;
using wrench.auto.repair.core.Messages;

namespace wrench.auto.repair.cadastro.application.Queries
{
    public class ObterClientePorIdQuery(Guid clienteId) : Command<ClienteViewModel>
    {
        public Guid ClienteId { get; private set; } = clienteId;

        public override bool EhValido()
        {
            ValidationResult = new ObterClientePorIdQueryValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class ObterClientePorIdQueryValidator : AbstractValidator<ObterClientePorIdQuery>
    {
        public static string ClienteIdVazio =>
            "O identificador do cliente não pode ser vazio";

        public ObterClientePorIdQueryValidator()
        {
            RuleFor(c => c.ClienteId)
                .NotEmpty()
                .WithMessage(ClienteIdVazio);
        }
    }
}
