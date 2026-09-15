using Microsoft.EntityFrameworkCore;
using wrench.auto.repair.estoque.domain.Data;
using wrench.auto.repair.estoque.domain.Entities;
using wrench.auto.repair.estoque.infra.Context;

namespace wrench.auto.repair.estoque.infra.Repositories;

public class PecaRepository(PecaDbContext _context) : Repository<Peca>(_context), IPecaRepository
{
    public async Task<IEnumerable<Peca>> ConsultaPorNomeAsync(string nomePeca, CancellationToken cancellationToken)
    {
        return await _context.Pecas.Where(p => EF.Functions.ILike(p.Nome, $"%{nomePeca}%")).ToListAsync(cancellationToken);
    }

    public async Task<Peca?> ObterPorNomeAsync(string nomePeca, CancellationToken cancellationToken)
    {
        return await _context.Pecas
            .FirstOrDefaultAsync(p => p.Nome.ToLower() == nomePeca.ToLower(), cancellationToken);
    }
}