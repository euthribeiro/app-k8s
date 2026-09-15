using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using wrench.auto.repair.ordem.servico.infra.Context;

namespace wrench.auto.repair.ordem.servico.infra.Seeds
{
    public static class OrdemServicoFunctionSeed
    {
        public static async Task CriarFuncaoDateDiffMillisecondsAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<OrdemServicoDbContext>();

            await db.Database.ExecuteSqlRawAsync(@"
                CREATE OR REPLACE FUNCTION datediff_milliseconds(start_ts timestamptz, end_ts timestamptz)
                RETURNS double precision AS $$
                SELECT EXTRACT(EPOCH FROM (end_ts - start_ts)) * 1000;
                $$ LANGUAGE sql IMMUTABLE;
            ");
        }
    }
}
