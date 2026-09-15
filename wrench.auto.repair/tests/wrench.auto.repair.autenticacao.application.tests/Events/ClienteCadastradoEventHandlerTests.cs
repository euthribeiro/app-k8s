using System.Linq.Expressions;
using Moq;
using wrench.auto.repair.autenticacao.application.Commands;
using wrench.auto.repair.autenticacao.application.Events;
using wrench.auto.repair.autenticacao.domain.Data;
using wrench.auto.repair.autenticacao.domain.Entities;
using wrench.auto.repair.core.Errors;
using wrench.auto.repair.core.Mediator;
using wrench.auto.repair.core.Messages.CommonMessages.IntegrationEvents;

namespace wrench.auto.repair.autenticacao.application.tests.Events
{
    public class ClienteCadastradoEventHandlerTests
    {
        [Fact(DisplayName = "Cliente cadastrado deve disparar criação de usuário quando perfil Cliente existir")]
        [Trait("Autenticacao", "Application")]
        public async Task Handle_DeveEnviarCriarUsuario_QuandoPerfilClienteExistir()
        {
            var perfilCliente = new Perfil("Cliente", "Cliente padrão", true, DateTime.UtcNow);
            var repo = new Mock<IUsuarioRepository>();
            repo.Setup(r => r.BuscarPerfil(It.IsAny<Expression<Func<Perfil, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { perfilCliente });

            var mediator = new Mock<IMediatorHandler>();
            mediator.Setup(m => m.EnviarComando<CriarUsuarioCommand, Guid>(It.IsAny<CriarUsuarioCommand>()))
                .ReturnsAsync(Result<Guid>.Created(Guid.NewGuid()));

            var handler = new ClienteCadastradoEventHandler(mediator.Object, repo.Object);
            var evt = new ClienteCadastradoEvent(Guid.NewGuid(), "novo@cliente.com");

            await handler.Handle(evt, CancellationToken.None);

            mediator.Verify(m => m.EnviarComando<CriarUsuarioCommand, Guid>(It.Is<CriarUsuarioCommand>(c =>
                c.Email == "novo@cliente.com" &&
                c.PerfilId == perfilCliente.Id &&
                c.RequerSenha == false)), Times.Once);
        }

        [Fact(DisplayName = "Cliente cadastrado não deve enviar comando quando não houver perfil Cliente")]
        [Trait("Autenticacao", "Application")]
        public async Task Handle_NaoDeveEnviarComando_QuandoNaoHouverPerfil()
        {
            var repo = new Mock<IUsuarioRepository>();
            repo.Setup(r => r.BuscarPerfil(It.IsAny<Expression<Func<Perfil, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<Perfil>());

            var mediator = new Mock<IMediatorHandler>();
            var handler = new ClienteCadastradoEventHandler(mediator.Object, repo.Object);

            await handler.Handle(new ClienteCadastradoEvent(Guid.NewGuid(), "x@y.com"), CancellationToken.None);

            mediator.Verify(m => m.EnviarComando<CriarUsuarioCommand, Guid>(It.IsAny<CriarUsuarioCommand>()), Times.Never);
        }
    }
}
