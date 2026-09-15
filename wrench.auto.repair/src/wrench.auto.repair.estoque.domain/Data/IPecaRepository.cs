using wrench.auto.repair.core.Data;
using wrench.auto.repair.estoque.domain.Entities;

namespace wrench.auto.repair.estoque.domain.Data;

public interface IPecaRepository : IRepository<Peca>
{
    Task<Peca?> ObterPorNomeAsync(string nomePeca, CancellationToken cancellationToken);
    Task<IEnumerable<Peca>> ConsultaPorNomeAsync(string nomePeca, CancellationToken cancellationToken);

}