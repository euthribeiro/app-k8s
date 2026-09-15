using wrench.auto.repair.autenticacao.application.Queries.ViewModels;
using wrench.auto.repair.core.Messages;

namespace wrench.auto.repair.autenticacao.application.Queries
{
    public class ObterTodosPerfisQuery : Command<IEnumerable<PerfilViewModel>>
    {
        public override bool EhValido()
        {
            return true;
        }
    }
}
