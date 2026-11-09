using wrench.auto.repair.core.DomainObjects;
using wrench.auto.repair.core.Extensions;
using wrench.auto.repair.core.Services;
using wrench.auto.repair.ordem.servico.domain.Enums;
using wrench.auto.repair.ordem.servico.domain.ValueObjects;

namespace wrench.auto.repair.ordem.servico.domain.Entities
{
    public class OrdemServico : Entity, IAggregateRoot
    {
        public Guid ClienteId { get; private set; }
        public Guid VeiculoId { get; set; }
        public string Descricao { get; private set; }
        public DateTime DataCriacao { get; private set; }
        public OrdemServicoStatus Status { get; private set; }
        public decimal ValorServico { get; private set; }
        public string? SolucaoProposta { get; private set; }
        public StatusAprovacaoEnum? StatusAprovacao { get; private set; }
        public string? MotivoRecusa { get; private set; }
        public DateTime? DataDiagnostico { get; private set; }
        public DateTime? DataEnvio { get; private set; }
        public DateTime? DataAprovacaoRecusa { get; private set; }
        public DateTime? DataFinalizacao { get; private set; }
        public DateTime? DataEntrega { get; private set; }

        private List<ItemOrdemServico> _pecas = [];
        public IReadOnlyCollection<ItemOrdemServico> Pecas => _pecas.AsReadOnly();

        private List<OrdemServicoHistorico> _historico = [];
        public IReadOnlyCollection<OrdemServicoHistorico> Historico => _historico.AsReadOnly();

        private static readonly OrdemServicoStatus[] StatusQuePermitemCancelamento =
        [
            OrdemServicoStatus.Recebida,
            OrdemServicoStatus.EmDiagnostico,
            OrdemServicoStatus.AguardandoAprovacao,
            OrdemServicoStatus.EmExecucao
        ];

        // Usado apenas pelo EF ao materializar a entidade: o construtor público registra a
        // entrada inicial do histórico e não pode rodar a cada leitura do banco.
        private OrdemServico()
        {
            Descricao = string.Empty;
        }

        public OrdemServico(Guid clienteId, Guid veiculoId, string descricao, OrdemServicoStatus status, DateTime dataCriacao)
        {
            Validar(clienteId, veiculoId, descricao, status);

            ClienteId = clienteId;
            VeiculoId = veiculoId;
            Descricao = descricao.RemoverAcentos().RemoverEspacosDuplicados().ToUpperInvariant();
            DataCriacao = dataCriacao;
            Status = status;

            SolucaoProposta = string.Empty;
            MotivoRecusa = string.Empty;
            ValorServico = 0;
            StatusAprovacao = StatusAprovacaoEnum.Indefinido;
            DataDiagnostico = dataCriacao;

            RegistrarTransicao(null, status, null, null);
        }

        public void AdicionarDiagnostico(string solucao, decimal valorServico, string? correlationId = null)
        {
            if (Status != OrdemServicoStatus.EmDiagnostico)
                throw new DomainException("A ordem de serviço não está em um status que permite diagnóstico.");

            Validacoes.ValidarSeMenorQue(valorServico, 0, "O valor do serviço deve ser maior ou igual a zero");
            Validacoes.ValidarSeVazio(solucao, "A solução proposta não pode ser vazia");

            var statusAnterior = Status;

            ValorServico = valorServico;
            SolucaoProposta = solucao;
            DataDiagnostico = DateTime.UtcNow;
            DataEnvio = DateTime.UtcNow;
            StatusAprovacao = StatusAprovacaoEnum.EmAnalise;
            Status = OrdemServicoStatus.AguardandoAprovacao;

            RegistrarTransicao(statusAnterior, Status, null, correlationId);
        }

        public void SolicitaDiagnostico(string? correlationId = null)
        {
            if (Status != OrdemServicoStatus.Recebida)
                throw new DomainException("A ordem de serviço não está em um status que permite solicitar diagnóstico.");

            var statusAnterior = Status;
            Status = OrdemServicoStatus.EmDiagnostico;

            RegistrarTransicao(statusAnterior, Status, null, correlationId);
        }

        public void AprovarOrcamento(string? correlationId = null)
        {
            if (Status != OrdemServicoStatus.AguardandoAprovacao)
                throw new DomainException("A ordem de serviço não está em um status que permite aprovar ordem de serviço.");

            var statusAnterior = Status;

            StatusAprovacao = StatusAprovacaoEnum.Aprovada;
            Status = OrdemServicoStatus.EmExecucao;
            DataAprovacaoRecusa = DateTime.UtcNow;

            RegistrarTransicao(statusAnterior, Status, null, correlationId);
        }

        public void RecusarOrcamento(string motivo, string? correlationId = null)
        {
            if (Status != OrdemServicoStatus.AguardandoAprovacao)
                throw new DomainException("A ordem de serviço não está em um status que permite recusar ordem de serviço.");

            Validacoes.ValidarSeVazio(motivo, "O motivo da recusa deve ser informado");

            var statusAnterior = Status;

            StatusAprovacao = StatusAprovacaoEnum.Recusada;
            Status = OrdemServicoStatus.Finalizada;
            DataAprovacaoRecusa = DateTime.UtcNow;
            MotivoRecusa = motivo;

            RegistrarTransicao(statusAnterior, Status, motivo, correlationId);
        }

        public void FinalizarOrdemServico(string? correlationId = null)
        {
            if (Status != OrdemServicoStatus.EmExecucao)
                throw new DomainException("A ordem de serviço não está em um status que permite finalização.");

            var statusAnterior = Status;

            DataFinalizacao = DateTime.UtcNow;
            Status = OrdemServicoStatus.Finalizada;

            RegistrarTransicao(statusAnterior, Status, null, correlationId);
        }

        public void EntregarServico(string? correlationId = null)
        {
            if (Status != OrdemServicoStatus.Finalizada)
                throw new DomainException("A ordem de serviço não está em um status que permite finalização.");

            var statusAnterior = Status;

            DataEntrega = DateTime.UtcNow;
            Status = OrdemServicoStatus.Entregue;

            RegistrarTransicao(statusAnterior, Status, null, correlationId);
        }

        public bool PodeSerCancelada() => StatusQuePermitemCancelamento.Contains(Status);

        public void Cancelar(string motivo, string? correlationId = null)
        {
            if (!PodeSerCancelada())
                throw new DomainException("A ordem de serviço não está em um status que permite cancelamento.");

            Validacoes.ValidarSeVazio(motivo, "O motivo do cancelamento deve ser informado");

            var statusAnterior = Status;
            Status = OrdemServicoStatus.Cancelada;

            RegistrarTransicao(statusAnterior, Status, motivo, correlationId);
        }

        public bool PodeRetornarParaDiagnostico() =>
            Status is OrdemServicoStatus.EmDiagnostico or OrdemServicoStatus.AguardandoAprovacao;

        public void RetornarParaDiagnostico(string motivo, string? correlationId = null)
        {
            if (!PodeRetornarParaDiagnostico())
                throw new DomainException("A ordem de serviço não está em um status que permite retornar para diagnóstico.");

            Validacoes.ValidarSeVazio(motivo, "O motivo do retorno para diagnóstico deve ser informado");

            var statusAnterior = Status;

            StatusAprovacao = StatusAprovacaoEnum.Indefinido;
            Status = OrdemServicoStatus.EmDiagnostico;

            RegistrarTransicao(statusAnterior, Status, motivo, correlationId);
        }

        public void AdicionarPeca(ItemOrdemServico item)
        {
            var itemExistente = _pecas.FirstOrDefault(i => i.PecaId == item.PecaId);

            if (itemExistente != null)
            {
                _pecas.Remove(item);
                itemExistente.AdicionarUnidades(item.Quantidade);
                _pecas.Add(itemExistente);
            }
            else
            {
                _pecas.Add(item);
            }
        }

        public decimal CalcularValorTotal()
        {
            return ValorServico + Pecas.Sum(p => p.CalcularValorTotalPeca());
        }

        public double? CalculaTempoExecucaoOrdem()
        {
            if (!DataFinalizacao.HasValue) return null;

            var diff = DataCriacao - DataFinalizacao.Value;

            return diff.TotalMilliseconds;
        }

        private void RegistrarTransicao(OrdemServicoStatus? statusAnterior, OrdemServicoStatus statusNovo, string? motivo, string? correlationId)
        {
            _historico.Add(new OrdemServicoHistorico(
                statusAnterior,
                statusNovo,
                motivo,
                correlationId ?? CorrelationContext.Current,
                DateTime.UtcNow));
        }

        private static void Validar(Guid clienteId, Guid veiculoId, string descricao, OrdemServicoStatus status)
        {
            Validacoes.ValidarSeVazio(clienteId, "O identificador do cliente não pode ser vazio");
            Validacoes.ValidarSeVazio(veiculoId, "O identificador do veículo não pode ser vazio");
            Validacoes.ValidarSeVazio(descricao, "A descrição não pode ser vazia");
            Validacoes.ValidarSeMenorQue((int)status, 1, "O status precisa ser informado");
        }
    }
}
