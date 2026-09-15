using wrench.auto.repair.ordem.servico.application.UseCases.OrcamentoUseCase;

namespace wrench.web.api.Models.OrdemServico
{
    public class AprovarOrcamentoRequest
    {
        public Guid OrdemServicoId { get; set; }

        public AprovarOrcamentoRequest(Guid ordemServicoId)
        {
            OrdemServicoId = ordemServicoId;
        }

        public static implicit operator AprovaOrcamentoCommand(AprovarOrcamentoRequest request)
        {
            return new AprovaOrcamentoCommand(request.OrdemServicoId);
        }
    }
}
