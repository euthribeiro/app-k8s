using wrench.auto.repair.autenticacao.domain.Entities;
using wrench.auto.repair.autenticacao.domain.Models;

namespace wrench.auto.repair.autenticacao.domain.Security
{
    public interface IJwtTokenGenerator
    {
        TokenAcesso GerarToken(Usuario usuario);
    }
}
