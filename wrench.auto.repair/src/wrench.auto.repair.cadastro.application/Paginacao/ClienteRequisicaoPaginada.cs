using FluentValidation;
using wrench.auto.repair.core.Pagination;

namespace wrench.auto.repair.cadastro.application.Paginacao
{
    public class ClienteRequisicaoPaginada : RequisicaoPaginada
    {
        public static readonly string[] OrdenacoesPermitidas = ["Nome", "Documento", "DataCadastro"];

        public ClienteRequisicaoPaginada()
            : base(OrdenacoesPermitidas)
        {

        }
    }

    public class ClienteRequisicaoPaginadaValidator : AbstractValidator<ClienteRequisicaoPaginada>
    {
        public ClienteRequisicaoPaginadaValidator()
        {
            When(c => !string.IsNullOrWhiteSpace(c.OrdenarPor), () =>
            {
                var message = $"A ordenação só é permitida para os campos: {string.Join(", ", ClienteRequisicaoPaginada.OrdenacoesPermitidas)}";

                RuleFor(c => c.OrdenarPor)
                    .Must(valor =>
                        string.IsNullOrWhiteSpace(valor) ||
                        ClienteRequisicaoPaginada.OrdenacoesPermitidas.Contains(valor.Trim(), StringComparer.InvariantCultureIgnoreCase))
                    .WithMessage(message);
            });

        }
    }
}
