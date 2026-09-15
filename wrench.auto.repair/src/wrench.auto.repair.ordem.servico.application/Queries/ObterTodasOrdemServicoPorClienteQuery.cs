using FluentValidation;
using wrench.auto.repair.core.Messages;
using wrench.auto.repair.core.Pagination;
using wrench.auto.repair.ordem.servico.application.Paginacao;
using wrench.auto.repair.ordem.servico.application.Queries.ViewModels;

namespace wrench.auto.repair.ordem.servico.application.Queries
{
    public class ObterTodasOrdemServicoPorClienteQuery(Guid clienteId, Guid? veiculoId, OrdemServicoRequisicaoPaginada paginacao) : Command<ResultadoPaginado<OrdemServicoViewModel>>
    {
        public Guid ClienteId { get; private set; } = clienteId;
        public Guid? VeiculoId { get; private set; } = veiculoId;
        public OrdemServicoRequisicaoPaginada Paginacao { get; private set; } = paginacao;

        public override bool EhValido()
        {
            ValidationResult = new ObterTodasOrdemServicoPorClienteQueryValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class ObterTodasOrdemServicoPorClienteQueryValidator : AbstractValidator<ObterTodasOrdemServicoPorClienteQuery>
    {
        public static string ClienteIdVazio => "O identificador do cliente precisa ser informado";
        public static string VeiculoIdVazio => "O identificador do veículo não é válido";

        public ObterTodasOrdemServicoPorClienteQueryValidator()
        {
            RuleFor(c => c.ClienteId)
                .NotEmpty()
                .WithMessage(ClienteIdVazio);

            When(c => c.VeiculoId != null, () =>
            {
                RuleFor(c => c.VeiculoId)
                 .NotEmpty()
                 .WithMessage(VeiculoIdVazio);
            });

            RuleFor(c => c.Paginacao)
                .SetValidator(new OrdemServicoRequisicaoPaginadaValidator());
        }
    }
}
