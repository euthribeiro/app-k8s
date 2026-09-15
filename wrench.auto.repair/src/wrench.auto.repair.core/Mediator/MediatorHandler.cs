using MediatR;
using wrench.auto.repair.core.Errors;
using wrench.auto.repair.core.Messages;
using wrench.auto.repair.core.Messages.CommonMessages.IntegratedQueries;

namespace wrench.auto.repair.core.Mediator
{
    public class MediatorHandler(IMediator _mediator) : IMediatorHandler
    {
        public async Task<Result> EnviarComando<T>(T comando) where T : Command
        {
            return await _mediator.Send(comando);
        }

        public async Task<Result<TOut>> EnviarComando<T, TOut>(T comando)
            where T : Command<TOut>
        {
            return await _mediator.Send(comando);
        }

        public async Task PublicarEvento<T>(T evento) where T : Event
        {
            await _mediator.Publish(evento);
        }

        public async Task<Result> ConsultaIntegrada<T>(T consulta) where T : IntegratedQuery
        {
            return await _mediator.Send(consulta);
        }

        public async Task<Result<TOut>> ConsultaIntegrada<T, TOut>(T consulta) where T : IntegratedQuery<TOut>
        {
            return await _mediator.Send(consulta);
        }
    }
}
