using FluentValidation;
using wrench.auto.repair.cadastro.application.Paginacao;
using wrench.auto.repair.cadastro.application.Queries.ViewModels;
using wrench.auto.repair.core.Messages;
using wrench.auto.repair.core.Pagination;

namespace wrench.auto.repair.cadastro.application.Queries
{
    public class ObterTodosVeiculosQuery(VeiculoRequisicaoPaginada paginacao) : Command<ResultadoPaginado<VeiculoViewModel>>
    {
        public VeiculoRequisicaoPaginada Paginacao { get; private set; } = paginacao;

        public override bool EhValido()
        {
            ValidationResult = new ObterTodosVeiculosQueryValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class ObterTodosVeiculosQueryValidator : AbstractValidator<ObterTodosVeiculosQuery>
    {
        public ObterTodosVeiculosQueryValidator()
        {
            RuleFor(c => c.Paginacao)
                 .SetValidator(new VeiculoRequisicaoPaginadaValidator());
        }
    }
}
