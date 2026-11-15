using wrench.auto.repair.ordem.servico.domain.Enums;

namespace wrench.auto.repair.ordem.servico.domain.ValueObjects
{
    public class OrdemServicoHistorico
    {
        public OrdemServicoHistorico(
            OrdemServicoStatus? statusAnterior,
            OrdemServicoStatus statusNovo,
            string? motivo,
            string? correlationId,
            DateTime ocorridoEm)
        {
            StatusAnterior = statusAnterior;
            StatusNovo = statusNovo;
            Motivo = motivo;
            CorrelationId = correlationId;
            OcorridoEm = ocorridoEm;
        }

        public OrdemServicoStatus? StatusAnterior { get; private set; }
        public OrdemServicoStatus StatusNovo { get; private set; }
        public string? Motivo { get; private set; }
        public string? CorrelationId { get; private set; }
        public DateTime OcorridoEm { get; private set; }
    }
}
