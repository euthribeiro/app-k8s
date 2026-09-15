using FluentValidation;
using wrench.auto.repair.core.Pagination;

namespace wrench.auto.repair.autenticacao.application.Paginacao
{
    public class UsuarioRequisicaoPaginada : RequisicaoPaginada
    {
        public static readonly string[] OrdenacoesPermitidas = ["Perfil", "DataCadastro"];

        public UsuarioRequisicaoPaginada()
            : base(OrdenacoesPermitidas)
        {

        }
    }

    public class UsuarioRequisicaoPaginadaValidator : AbstractValidator<UsuarioRequisicaoPaginada>
    {
        public UsuarioRequisicaoPaginadaValidator()
        {
            When(c => !string.IsNullOrWhiteSpace(c.OrdenarPor), () =>
            {
                var message = $"A ordenação só é permitida para os campos: {string.Join(", ", UsuarioRequisicaoPaginada.OrdenacoesPermitidas)}";

                RuleFor(c => c.OrdenarPor)
                    .Must(valor =>
                        string.IsNullOrWhiteSpace(valor) ||
                        UsuarioRequisicaoPaginada.OrdenacoesPermitidas.Contains(valor.Trim(), StringComparer.InvariantCultureIgnoreCase))
                    .WithMessage(message);
            });

        }
    }
}
