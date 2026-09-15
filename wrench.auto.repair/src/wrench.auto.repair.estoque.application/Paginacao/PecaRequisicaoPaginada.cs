using FluentValidation;
using wrench.auto.repair.core.Pagination;

namespace wrench.auto.repair.estoque.application.Paginacao
{
    public class PecaRequisicaoPaginada : RequisicaoPaginada
    {
        public static readonly string[] OrdenacoesPermitidas = ["Nome", "Valor", "Quantidade", "DataCadastro"];

        public PecaRequisicaoPaginada()
            : base(OrdenacoesPermitidas)
        {

        }
    }

    public class PecaRequisicaoPaginadaValidator : AbstractValidator<PecaRequisicaoPaginada>
    {
        public PecaRequisicaoPaginadaValidator()
        {
            When(c => !string.IsNullOrWhiteSpace(c.OrdenarPor), () =>
            {
                var message = $"A ordenação só é permitida para os campos: {string.Join(", ", PecaRequisicaoPaginada.OrdenacoesPermitidas)}";


                RuleFor(c => c.OrdenarPor)
                    .Must(valor =>
                        string.IsNullOrWhiteSpace(valor) ||
                        PecaRequisicaoPaginada.OrdenacoesPermitidas.Contains(valor.Trim(), StringComparer.InvariantCultureIgnoreCase))
                    .WithMessage(message);
            });

        }
    }
}
