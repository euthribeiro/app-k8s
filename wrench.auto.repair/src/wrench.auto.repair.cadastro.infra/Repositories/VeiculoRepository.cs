using Microsoft.EntityFrameworkCore;
using wrench.auto.repair.cadastro.domain.Data;
using wrench.auto.repair.cadastro.domain.Entities;

namespace wrench.auto.repair.cadastro.infra.Repositories
{
    public class VeiculoRepository(CadastroContext _context) :
        Repository<Veiculo>(_context),
        IVeiculoRepository
    {
        public async Task<Veiculo?> ObterVeiculoPelaPlacaAsync(string placa, CancellationToken cancellationToken)
        {
            return await DbSet.FirstOrDefaultAsync(v => v.PlacaDoVeiculo == placa, cancellationToken);
        }
    }
}
