using wrench.auto.repair.core.DomainObjects;
using wrench.auto.repair.core.Extensions;
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
        }

        public void AdicionarDiagnostico(string solucao, decimal valorServico)
        {
            if (Status != OrdemServicoStatus.EmDiagnostico)
                throw new DomainException("A ordem de serviço não está em um status que permite diagnóstico.");

            Validacoes.ValidarSeMenorQue(valorServico, 0, "O valor do serviço deve ser maior ou igual a zero");
            Validacoes.ValidarSeVazio(solucao, "A solução proposta não pode ser vazia");

            ValorServico = valorServico;
            SolucaoProposta = solucao;
            DataDiagnostico = DateTime.UtcNow;
            DataEnvio = DateTime.UtcNow;
            StatusAprovacao = StatusAprovacaoEnum.EmAnalise;
            Status = OrdemServicoStatus.AguardandoAprovacao;
        }

        public void SolicitaDiagnostico()
        {
            if (Status != OrdemServicoStatus.Recebida)
                throw new DomainException("A ordem de serviço não está em um status que permite solicitar diagnóstico.");

            Status = OrdemServicoStatus.EmDiagnostico;
        }

        public void AprovarOrcamento()
        {
            if (Status != OrdemServicoStatus.AguardandoAprovacao)
                throw new DomainException("A ordem de serviço não está em um status que permite aprovar ordem de serviço.");

            StatusAprovacao = StatusAprovacaoEnum.Aprovada;
            Status = OrdemServicoStatus.EmExecucao;
            DataAprovacaoRecusa = DateTime.UtcNow;
        }

        public void RecusarOrcamento(string motivo)
        {
            if (Status != OrdemServicoStatus.AguardandoAprovacao)
                throw new DomainException("A ordem de serviço não está em um status que permite recusar ordem de serviço.");

            Validacoes.ValidarSeVazio(motivo, "O motivo da recusa deve ser informado");

            StatusAprovacao = StatusAprovacaoEnum.Recusada;
            Status = OrdemServicoStatus.Finalizada;
            DataAprovacaoRecusa = DateTime.UtcNow;
            MotivoRecusa = motivo;
        }

        public void FinalizarOrdemServico()
        {
            if (Status != OrdemServicoStatus.EmExecucao)
                throw new DomainException("A ordem de serviço não está em um status que permite finalização.");
            DataFinalizacao = DateTime.UtcNow;
            Status = OrdemServicoStatus.Finalizada;
        }

        public void EntregarServico()
        {
            if (Status != OrdemServicoStatus.Finalizada)
                throw new DomainException("A ordem de serviço não está em um status que permite finalização.");

            DataEntrega = DateTime.UtcNow;
            Status = OrdemServicoStatus.Entregue;
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

        private static void Validar(Guid clienteId, Guid veiculoId, string descricao, OrdemServicoStatus status)
        {
            Validacoes.ValidarSeVazio(clienteId, "O identificador do cliente não pode ser vazio");
            Validacoes.ValidarSeVazio(veiculoId, "O identificador do veículo não pode ser vazio");
            Validacoes.ValidarSeVazio(descricao, "A descrição não pode ser vazia");
            Validacoes.ValidarSeMenorQue((int)status, 1, "O status precisa ser informado");
        }
    }
}
