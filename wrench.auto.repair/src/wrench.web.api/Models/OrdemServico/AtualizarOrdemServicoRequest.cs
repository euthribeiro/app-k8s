using wrench.auto.repair.ordem.servico.application.UseCases.OrdemServicoUseCase;

namespace wrench.web.api.Models.OrdemServico
{
    public class AtualizarOrdemServicoRequest
    {
        public Guid OrdemServicoId { get; set; }

        public AtualizarOrdemServicoRequest(Guid ordemServicoId)
        {
            OrdemServicoId = ordemServicoId;
        }

        public static implicit operator FinalizarOrdemServicoCommand(AtualizarOrdemServicoRequest request)
        {
            return new FinalizarOrdemServicoCommand(request.OrdemServicoId);
        }
    }
}
