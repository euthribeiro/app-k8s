using FluentValidation;
using wrench.auto.repair.core.Pagination;

namespace wrench.auto.repair.cadastro.application.Paginacao
{
    public class VeiculoRequisicaoPaginada : RequisicaoPaginada
    {
        public static readonly string[] OrdenacoesPermitidas = ["Marca", "Modelo", "Cor", "AnoFabricacao", "AnoModelo", "PlacaDoVeiculo", "UltimaRevisao", "QuilometragemAtual", "DataCadastro"];

        public VeiculoRequisicaoPaginada()
            : base(OrdenacoesPermitidas)
        {

        }
    }

    public class VeiculoRequisicaoPaginadaValidator : AbstractValidator<VeiculoRequisicaoPaginada>
    {
        public VeiculoRequisicaoPaginadaValidator()
        {
            When(c => !string.IsNullOrWhiteSpace(c.OrdenarPor), () =>
            {
                var message = $"A ordenação só é permitida para os campos: {string.Join(", ", VeiculoRequisicaoPaginada.OrdenacoesPermitidas)}";

                RuleFor(c => c.OrdenarPor)
                    .Must(valor =>
                        string.IsNullOrWhiteSpace(valor) ||
                        VeiculoRequisicaoPaginada.OrdenacoesPermitidas.Contains(valor.Trim(), StringComparer.InvariantCultureIgnoreCase))
                    .WithMessage(message);
            });

        }
    }
}
