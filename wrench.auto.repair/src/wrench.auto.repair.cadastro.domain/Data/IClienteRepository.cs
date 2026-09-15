using wrench.auto.repair.cadastro.domain.Entities;
using wrench.auto.repair.core.Data;
using wrench.auto.repair.core.ValueObjects;

namespace wrench.auto.repair.cadastro.domain.Data
{
    public interface IClienteRepository : IRepository<Cliente>
    {
        Task<Cliente?> ObterPorDocumentAsync(string documento, CancellationToken cancellationToken);
        Task<Cliente?> ObterPorEmailAsync(Email email, CancellationToken cancellationToken);
    }
}
