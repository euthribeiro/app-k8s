using MediatR;
using wrench.auto.repair.autenticacao.application.Commands;
using wrench.auto.repair.autenticacao.domain.Data;
using wrench.auto.repair.core.Mediator;
using wrench.auto.repair.core.Messages.CommonMessages.IntegrationEvents;

namespace wrench.auto.repair.autenticacao.application.Events
{
    public class ClienteCadastradoEventHandler(
        IMediatorHandler _mediatorHandler,
        IUsuarioRepository _usuarioRepository
    ) : INotificationHandler<ClienteCadastradoEvent>
    {
        public async Task Handle(ClienteCadastradoEvent notification, CancellationToken cancellationToken)
        {
            var perfis = await _usuarioRepository.BuscarPerfil(p => p.Nome == "Cliente", cancellationToken);

            if (!perfis.Any()) return;

            var perfilCliente = perfis.First();

            var criarUsuarioCommand = new CriarUsuarioCommand(notification.Email, null, perfilCliente.Id, true, requerSenha: false);

            await _mediatorHandler.EnviarComando<CriarUsuarioCommand, Guid>(criarUsuarioCommand);
        }
    }
}
