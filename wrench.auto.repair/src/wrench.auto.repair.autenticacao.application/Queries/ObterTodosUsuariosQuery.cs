using FluentValidation;
using wrench.auto.repair.autenticacao.application.Paginacao;
using wrench.auto.repair.autenticacao.application.Queries.ViewModels;
using wrench.auto.repair.core.Messages;
using wrench.auto.repair.core.Pagination;

namespace wrench.auto.repair.autenticacao.application.Queries
{
    public class ObterTodosUsuariosQuery(UsuarioRequisicaoPaginada paginacao) : Command<ResultadoPaginado<UsuarioViewModel>>
    {
        public UsuarioRequisicaoPaginada Paginacao { get; private set; } = paginacao;

        public override bool EhValido()
        {
            ValidationResult = new ObterTodosUsuariosQueryValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class ObterTodosUsuariosQueryValidator : AbstractValidator<ObterTodosUsuariosQuery>
    {
        public ObterTodosUsuariosQueryValidator()
        {
            RuleFor(c => c.Paginacao)
                .SetValidator(new UsuarioRequisicaoPaginadaValidator());
        }
    }
}
