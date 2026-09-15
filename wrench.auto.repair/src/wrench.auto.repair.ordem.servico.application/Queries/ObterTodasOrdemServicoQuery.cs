using FluentValidation;
using wrench.auto.repair.core.Messages;
using wrench.auto.repair.core.Pagination;
using wrench.auto.repair.ordem.servico.application.Paginacao;
using wrench.auto.repair.ordem.servico.application.Queries.ViewModels;

namespace wrench.auto.repair.ordem.servico.application.Queries
{
    public class ObterTodasOrdemServicoQuery(OrdemServicoRequisicaoPaginada paginacao) : Command<ResultadoPaginado<OrdemServicoViewModel>>
    {
        public OrdemServicoRequisicaoPaginada Paginacao { get; private set; } = paginacao;

        public override bool EhValido()
        {
            ValidationResult = new ObterTodasOrdemServicoQueryValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class ObterTodasOrdemServicoQueryValidator : AbstractValidator<ObterTodasOrdemServicoQuery>
    {
        public ObterTodasOrdemServicoQueryValidator()
        {
            RuleFor(c => c.Paginacao)
                .SetValidator(new OrdemServicoRequisicaoPaginadaValidator());
        }
    }
}
