using Microsoft.EntityFrameworkCore;
using Wrench.Contracts;
using wrench.auto.repair.ordem.servico.domain.Saga.Mensageria;
using wrench.auto.repair.ordem.servico.infra.Context;

namespace wrench.auto.repair.ordem.servico.infra.Saga
{
    /// <summary>
    /// Outbox sobre o mesmo <c>DbContext</c> da ordem de serviço: a mensagem entra no
    /// <c>ChangeTracker</c> e só existe no banco se a transação que mudou o estado também existir.
    /// </summary>
    public class OutboxSaga(OrdemServicoDbContext contexto, TimeProvider relogio) : IOutboxSaga
    {
        public void Enfileirar<TContrato>(string tipo, TContrato conteudo) where TContrato : ISagaMensagem
        {
            var mensagem = MensagemSaga.Criar(tipo, conteudo, relogio.GetUtcNow().UtcDateTime);

            contexto.Set<OutboxMensagem>().Add(new OutboxMensagem
            {
                Id = mensagem.Id,
                OrdemServicoId = mensagem.OrdemServicoId,
                Tipo = mensagem.Tipo,
                Payload = mensagem.Payload,
                CorrelationId = mensagem.CorrelationId,
                Traceparent = mensagem.Traceparent,
                CriadaEm = mensagem.CriadaEm
            });
        }

        public async Task<IReadOnlyList<MensagemSaga>> ObterPendentesAsync(int limite, CancellationToken cancellationToken) =>
            await contexto.Set<OutboxMensagem>()
                .AsNoTracking()
                .Where(mensagem => mensagem.PublicadaEm == null)
                .OrderBy(mensagem => mensagem.CriadaEm)
                .Take(limite)
                .Select(mensagem => new MensagemSaga(
                    mensagem.Id,
                    mensagem.OrdemServicoId,
                    mensagem.Tipo,
                    mensagem.Payload,
                    mensagem.CorrelationId,
                    mensagem.Traceparent,
                    mensagem.CriadaEm))
                .ToListAsync(cancellationToken);

        public async Task MarcarPublicadaAsync(Guid id, DateTime publicadaEm, CancellationToken cancellationToken) =>
            await contexto.Set<OutboxMensagem>()
                .Where(mensagem => mensagem.Id == id)
                .ExecuteUpdateAsync(
                    atualizacao => atualizacao.SetProperty(mensagem => mensagem.PublicadaEm, publicadaEm),
                    cancellationToken);

        public Task<int> ContarPendentesAsync(CancellationToken cancellationToken) =>
            contexto.Set<OutboxMensagem>()
                .AsNoTracking()
                .CountAsync(mensagem => mensagem.PublicadaEm == null, cancellationToken);
    }

    /// <summary>Inbox sobre o mesmo <c>DbContext</c>: o registro da mensagem consumida entra na transação do novo estado.</summary>
    public class InboxSaga(OrdemServicoDbContext contexto) : IInboxSaga
    {
        public Task<bool> JaProcessadaAsync(string messageId, CancellationToken cancellationToken) =>
            contexto.Set<InboxMensagem>()
                .AsNoTracking()
                .AnyAsync(mensagem => mensagem.MessageId == messageId, cancellationToken);

        public void Registrar(string messageId, string tipo, DateTime recebidaEm) =>
            contexto.Set<InboxMensagem>().Add(new InboxMensagem
            {
                MessageId = messageId,
                Tipo = tipo,
                RecebidaEm = recebidaEm
            });
    }
}
