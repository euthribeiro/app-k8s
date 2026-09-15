using FluentValidation;

namespace wrench.auto.repair.core.Messages.CommonMessages.IntegratedQueries
{
    public class VeiculoExisteEPertenteAoClienteQuery(Guid clienteId, Guid veiculoId) : IntegratedQuery
    {
        public Guid ClienteId { get; private set; } = clienteId;
        public Guid VeiculoId { get; private set; } = veiculoId;

        public override bool EhValido()
        {
            ValidationResult = new VeiculoExisteEPertenteAoClienteQueryValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class VeiculoExisteEPertenteAoClienteQueryValidator : AbstractValidator<VeiculoExisteEPertenteAoClienteQuery>
    {
        public static string ClienteIdVazio =>
            "O identificador do cliente não pode ser vazio";

        public static string VeiculoIdVazio =>
            "O identificador da veículo não pode ser vazio";

        public VeiculoExisteEPertenteAoClienteQueryValidator()
        {
            RuleFor(c => c.ClienteId)
             .NotEmpty()
             .WithMessage(ClienteIdVazio);

            RuleFor(c => c.VeiculoId)
             .NotEmpty()
             .WithMessage(VeiculoIdVazio);
        }
    }
}
