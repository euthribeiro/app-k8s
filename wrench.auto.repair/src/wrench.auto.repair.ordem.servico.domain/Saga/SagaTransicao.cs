namespace wrench.auto.repair.ordem.servico.domain.Saga
{
    /// <summary>Registro imutável de uma transição da Saga, para auditoria e para o histórico da OS.</summary>
    public class SagaTransicao
    {
        public Guid Id { get; private set; }
        public Guid SagaOrdemServicoId { get; private set; }
        public SagaEstado? EstadoAnterior { get; private set; }
        public SagaEstado EstadoNovo { get; private set; }
        public SagaEtapa Etapa { get; private set; }
        public string Gatilho { get; private set; }
        public string? Motivo { get; private set; }
        public DateTime OcorridaEm { get; private set; }

        private SagaTransicao()
        {
            Gatilho = string.Empty;
        }

        public SagaTransicao(
            SagaEstado? estadoAnterior,
            SagaEstado estadoNovo,
            SagaEtapa etapa,
            string gatilho,
            string? motivo,
            DateTime ocorridaEm)
        {
            Id = Guid.NewGuid();
            EstadoAnterior = estadoAnterior;
            EstadoNovo = estadoNovo;
            Etapa = etapa;
            Gatilho = gatilho;
            Motivo = motivo;
            OcorridaEm = ocorridaEm;
        }
    }
}
