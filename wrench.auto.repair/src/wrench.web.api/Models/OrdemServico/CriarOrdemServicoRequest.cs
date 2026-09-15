using wrench.auto.repair.ordem.servico.application.UseCases.OrdemServicoUseCase;

namespace wrench.web.api.Models.OrdemServico
{
    public class CriarOrdemServicoRequest
    {
        public Guid ClienteId { get; init; }
        public Guid VeiculoId { get; init; }
        public string Descricao { get; init; }

        public static explicit operator CriarOrdemServicoCommand(CriarOrdemServicoRequest request)
        {
            return new CriarOrdemServicoCommand(request.ClienteId, request.VeiculoId, request.Descricao);
        }
    }
}
