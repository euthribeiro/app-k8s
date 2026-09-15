using wrench.auto.repair.cadastro.domain.Entities;
using wrench.auto.repair.core.Data;

namespace wrench.auto.repair.cadastro.domain.Data
{
    public interface IVeiculoRepository : IRepository<Veiculo>
    {
        Task<Veiculo?> ObterVeiculoPelaPlacaAsync(string placa, CancellationToken cancellationToken);
    }
}
