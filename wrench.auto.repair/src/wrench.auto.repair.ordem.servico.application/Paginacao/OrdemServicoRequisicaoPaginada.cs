using FluentValidation;
using wrench.auto.repair.core.Pagination;

namespace wrench.auto.repair.ordem.servico.application.Paginacao
{
    public class OrdemServicoRequisicaoPaginada : RequisicaoPaginada
    {
        public static readonly string[] OrdenacoesPermitidas = ["DataCriacao", "Status", "ValorServico", "StatusAprovacao", "DataDiagnostico", "DataEnvio", "DataAprovacaoRecusa", "DataEntrega"];

        public OrdemServicoRequisicaoPaginada()
            : base(OrdenacoesPermitidas)
        {

        }
    }

    public class OrdemServicoRequisicaoPaginadaValidator : AbstractValidator<OrdemServicoRequisicaoPaginada>
    {
        public OrdemServicoRequisicaoPaginadaValidator()
        {
            When(c => !string.IsNullOrWhiteSpace(c.OrdenarPor), () =>
            {
                var message = $"A ordenação só é permitida para os campos: {string.Join(", ", OrdemServicoRequisicaoPaginada.OrdenacoesPermitidas)}";


                RuleFor(c => c.OrdenarPor)
                    .Must(valor =>
                        string.IsNullOrWhiteSpace(valor) ||
                        OrdemServicoRequisicaoPaginada.OrdenacoesPermitidas.Contains(valor.Trim(), StringComparer.InvariantCultureIgnoreCase))
                    .WithMessage(message);
            });

        }
    }
}
