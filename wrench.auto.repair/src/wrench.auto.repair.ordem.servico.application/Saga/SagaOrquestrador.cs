using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wrench.Contracts;
using wrench.auto.repair.core.Services;
using wrench.auto.repair.ordem.servico.domain.Saga.Mensageria;
using wrench.auto.repair.ordem.servico.domain.Data;
using wrench.auto.repair.ordem.servico.domain.Entities;
using wrench.auto.repair.ordem.servico.domain.Enums;
using wrench.auto.repair.ordem.servico.domain.Saga;

namespace wrench.auto.repair.ordem.servico.application.Saga
{
    /// <summary>
    /// Orquestrador da Saga da ordem de serviço. Traduz cada mensagem recebida em uma transição
    /// da máquina de estados, na mudança de status correspondente da OS e nos comandos de saída,
    /// tudo gravado numa única transação do banco junto com o outbox e o registro de idempotência.
    /// </summary>
    public class SagaOrquestrador(
        ISagaOrdemServicoRepository sagaRepository,
        IOrdemServicoRepository ordemServicoRepository,
        IOutboxSaga outbox,
        IInboxSaga inbox,
        IOptions<SagaOptions> opcoes,
        TimeProvider relogio,
        ILogger<SagaOrquestrador> logger) : ISagaOrquestrador
    {
        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

        private readonly SagaOptions _opcoes = opcoes.Value;

        public async Task IniciarAsync(OrdemServico ordemServico, string correlationId, CancellationToken cancellationToken)
        {
            var agora = relogio.GetUtcNow().UtcDateTime;

            var saga = new SagaOrdemServico(ordemServico.Id, correlationId, agora, _opcoes.PrazoOrcamento);

            await sagaRepository.Adicionar(saga, cancellationToken);

            outbox.Enfileirar(MensagemTipos.GerarOrcamento, new GerarOrcamento(
                ordemServico.Id,
                correlationId,
                ordemServico.ClienteId,
                ordemServico.Descricao,
                ordemServico.SolucaoProposta ?? string.Empty,
                ordemServico.ValorServico,
                [.. ordemServico.Pecas.Select(peca => new ItemOrcamentoDto(peca.PecaId, peca.Nome, peca.Quantidade, peca.ValorUnitario))]));

            SagaMetrics.SagaIniciada();

            logger.LogInformation(
                "Saga iniciada para a ordem de servico {OrdemServicoId} com correlacao {CorrelationId}.",
                ordemServico.Id, correlationId);
        }

        public async Task<ResultadoSaga> ProcessarAsync(MensagemRecebida mensagem, CancellationToken cancellationToken)
        {
            using var atividade = SagaMetrics.Source.StartActivity($"saga {mensagem.Tipo}", ActivityKind.Consumer);

            SagaMetrics.MensagemRecebida(mensagem.Tipo);

            if (await inbox.JaProcessadaAsync(mensagem.MessageId, cancellationToken))
            {
                logger.LogInformation("Mensagem {MessageId} ({Tipo}) ja processada; ignorada.", mensagem.MessageId, mensagem.Tipo);
                return ResultadoSaga.Ignorada;
            }

            CorrelationContext.Current = mensagem.CorrelationId;

            var resultado = await AplicarAsync(mensagem, atividade, cancellationToken);

            if (resultado != ResultadoSaga.Processada)
                return resultado;

            var agora = relogio.GetUtcNow().UtcDateTime;

            inbox.Registrar(mensagem.MessageId, mensagem.Tipo, agora);

            await sagaRepository.UnitOfWork.CommitAsync();

            return ResultadoSaga.Processada;
        }

        public async Task<int> AplicarTimeoutsAsync(CancellationToken cancellationToken)
        {
            var agora = relogio.GetUtcNow().UtcDateTime;

            var expiradas = await sagaRepository.ObterExpiradasAsync(agora, _opcoes.LoteTimeouts, cancellationToken);

            if (expiradas.Count == 0) return 0;

            var compensadas = 0;

            foreach (var saga in expiradas)
            {
                var estado = saga.Estado;

                var motivo = estado switch
                {
                    SagaEstado.AguardandoOrcamento => "O orcamento nao foi gerado dentro do prazo.",
                    SagaEstado.AguardandoAprovacao => "O cliente nao respondeu ao orcamento dentro do prazo.",
                    SagaEstado.AguardandoPagamento => "O pagamento nao foi confirmado dentro do prazo.",
                    SagaEstado.AguardandoExecucao => "A execucao nao foi concluida dentro do prazo.",
                    _ => "O estorno nao foi confirmado dentro do prazo."
                };

                CorrelationContext.Current = saga.CorrelationId;

                if (estado == SagaEstado.Compensando)
                {
                    if (!saga.AoFalharEstorno(motivo, agora)) continue;

                    logger.LogError(
                        "Estorno da ordem de servico {OrdemServicoId} expirou; a Saga exige tratamento manual.",
                        saga.OrdemServicoId);
                }
                else
                {
                    if (!await CompensarAsync(saga, "Timeout", motivo, agora, cancellationToken)) continue;
                }

                SagaMetrics.TimeoutDaEtapa(estado.ToString());
                await sagaRepository.Atualizar(saga);
                compensadas++;
            }

            if (compensadas > 0)
                await sagaRepository.UnitOfWork.CommitAsync();

            return compensadas;
        }

        private async Task<ResultadoSaga> AplicarAsync(MensagemRecebida mensagem, Activity? atividade, CancellationToken cancellationToken)
        {
            var agora = relogio.GetUtcNow().UtcDateTime;

            switch (mensagem.Tipo)
            {
                case MensagemTipos.OrcamentoCriado:
                    {
                        var evento = Desserializar<OrcamentoCriado>(mensagem);
                        return await ComSagaAsync(evento, atividade, cancellationToken, saga =>
                            Task.FromResult(saga.AoCriarOrcamento(evento.OrcamentoId, agora, _opcoes.PrazoAprovacao)));
                    }

                case MensagemTipos.OrcamentoFalhou:
                    {
                        var evento = Desserializar<OrcamentoFalhou>(mensagem);
                        return await ComSagaAsync(evento, atividade, cancellationToken, saga =>
                            CompensarAsync(saga, mensagem.Tipo, evento.Motivo, agora, cancellationToken));
                    }

                case MensagemTipos.OrcamentoAprovado:
                    {
                        var evento = Desserializar<OrcamentoAprovado>(mensagem);
                        return await ComSagaAsync(evento, atividade, cancellationToken, saga =>
                            Task.FromResult(saga.AoAprovarOrcamento(evento.OrcamentoId, evento.ValorAprovado, agora, _opcoes.PrazoPagamento)));
                    }

                case MensagemTipos.OrcamentoRecusado:
                    {
                        var evento = Desserializar<OrcamentoRecusado>(mensagem);
                        return await ComSagaAsync(evento, atividade, cancellationToken, saga =>
                            CompensarAsync(saga, mensagem.Tipo, evento.Motivo, agora, cancellationToken));
                    }

                case MensagemTipos.PagamentoAprovado:
                    {
                        var evento = Desserializar<PagamentoAprovado>(mensagem);
                        return await ComSagaAsync(evento, atividade, cancellationToken, saga =>
                            AoAprovarPagamentoAsync(saga, evento, agora, cancellationToken));
                    }

                case MensagemTipos.PagamentoFalhou:
                    {
                        var evento = Desserializar<PagamentoFalhou>(mensagem);
                        return await ComSagaAsync(evento, atividade, cancellationToken, saga =>
                            CompensarAsync(saga, mensagem.Tipo, evento.Motivo, agora, cancellationToken));
                    }

                case MensagemTipos.PagamentoEstornado:
                    {
                        var evento = Desserializar<PagamentoEstornado>(mensagem);
                        return await ComSagaAsync(evento, atividade, cancellationToken, saga =>
                            Task.FromResult(ConcluirCompensacao(saga, agora)));
                    }

                case MensagemTipos.EstornoFalhou:
                    {
                        var evento = Desserializar<EstornoFalhou>(mensagem);
                        return await ComSagaAsync(evento, atividade, cancellationToken, saga =>
                            Task.FromResult(saga.AoFalharEstorno(evento.Motivo, agora)));
                    }

                case MensagemTipos.ExecucaoConcluida:
                    {
                        var evento = Desserializar<ExecucaoConcluida>(mensagem);
                        return await ComSagaAsync(evento, atividade, cancellationToken, saga =>
                            AoConcluirExecucaoAsync(saga, agora, cancellationToken));
                    }

                case MensagemTipos.ExecucaoFalhou:
                    {
                        var evento = Desserializar<ExecucaoFalhou>(mensagem);
                        return await ComSagaAsync(evento, atividade, cancellationToken, saga =>
                            CompensarAsync(saga, mensagem.Tipo, evento.Motivo, agora, cancellationToken));
                    }

                default:
                    logger.LogWarning("Tipo de mensagem {Tipo} nao tratado pelo orquestrador.", mensagem.Tipo);
                    return ResultadoSaga.TipoDesconhecido;
            }
        }

        private async Task<ResultadoSaga> ComSagaAsync(
            ISagaMensagem mensagem,
            Activity? atividade,
            CancellationToken cancellationToken,
            Func<SagaOrdemServico, Task<bool>> transicao)
        {
            var saga = await sagaRepository.ObterPorOrdemServicoAsync(mensagem.OrdemServicoId, cancellationToken);

            if (saga is null)
            {
                logger.LogWarning("Nao existe Saga para a ordem de servico {OrdemServicoId}.", mensagem.OrdemServicoId);
                return ResultadoSaga.SagaNaoEncontrada;
            }

            atividade?.SetTag("saga.ordemServicoId", saga.OrdemServicoId);
            atividade?.SetTag("saga.estado", saga.Estado.ToString());
            atividade?.SetTag("saga.correlationId", saga.CorrelationId);

            if (!await transicao(saga))
            {
                logger.LogInformation(
                    "Transicao ignorada para a ordem de servico {OrdemServicoId}: a Saga esta em {Estado}.",
                    saga.OrdemServicoId, saga.Estado);

                return ResultadoSaga.Ignorada;
            }

            await sagaRepository.Atualizar(saga);

            atividade?.SetTag("saga.estadoFinal", saga.Estado.ToString());

            return ResultadoSaga.Processada;
        }

        private async Task<bool> AoAprovarPagamentoAsync(
            SagaOrdemServico saga,
            PagamentoAprovado evento,
            DateTime agora,
            CancellationToken cancellationToken)
        {
            if (!saga.AoAprovarPagamento(evento.PagamentoId, evento.Valor, agora, _opcoes.PrazoExecucao))
                return false;

            var ordemServico = await ordemServicoRepository.ObterPorIdAsync(saga.OrdemServicoId, cancellationToken);

            if (ordemServico is null)
                throw new InvalidOperationException($"Ordem de servico {saga.OrdemServicoId} nao encontrada para iniciar a execucao.");

            if (ordemServico.Status == OrdemServicoStatus.AguardandoAprovacao)
            {
                ordemServico.AprovarOrcamento(saga.CorrelationId);
                await ordemServicoRepository.Atualizar(ordemServico);
            }

            outbox.Enfileirar(MensagemTipos.IniciarExecucao, new IniciarExecucao(
                saga.OrdemServicoId,
                saga.CorrelationId,
                ordemServico.Descricao,
                [.. ordemServico.Pecas.Select(peca => new ItemExecucaoDto(peca.PecaId, peca.Nome, peca.Quantidade))],
                evento.Valor));

            return true;
        }

        private async Task<bool> AoConcluirExecucaoAsync(SagaOrdemServico saga, DateTime agora, CancellationToken cancellationToken)
        {
            if (!saga.AoConcluirExecucao(agora)) return false;

            var ordemServico = await ordemServicoRepository.ObterPorIdAsync(saga.OrdemServicoId, cancellationToken);

            if (ordemServico is not null && ordemServico.Status == OrdemServicoStatus.EmExecucao)
            {
                ordemServico.FinalizarOrdemServico(saga.CorrelationId);
                await ordemServicoRepository.Atualizar(ordemServico);
            }

            SagaMetrics.SagaConcluida((agora - saga.DataCriacao).TotalMilliseconds);

            return true;
        }

        /// <summary>
        /// Desfaz as etapas já concluídas. A etapa corrente decide o que compensar: quando o
        /// orçamento sequer foi gerado basta devolver a OS ao diagnóstico; quando já houve
        /// pagamento é preciso estornar no Billing e tirar a OS da fila de execução.
        /// </summary>
        private async Task<bool> CompensarAsync(
            SagaOrdemServico saga,
            string gatilho,
            string motivo,
            DateTime agora,
            CancellationToken cancellationToken)
        {
            var etapa = saga.Etapa;

            if (!saga.Compensar(gatilho, motivo, agora, _opcoes.PrazoEstorno)) return false;

            switch (etapa)
            {
                case SagaEtapa.Aprovacao:
                case SagaEtapa.Pagamento:
                    outbox.Enfileirar(MensagemTipos.CancelarOrcamento,
                        new CancelarOrcamento(saga.OrdemServicoId, saga.CorrelationId, motivo));
                    break;

                case SagaEtapa.Execucao:
                    outbox.Enfileirar(MensagemTipos.CancelarExecucao,
                        new CancelarExecucao(saga.OrdemServicoId, saga.CorrelationId, motivo));

                    if (saga.PagamentoId.HasValue)
                    {
                        outbox.Enfileirar(MensagemTipos.EstornarPagamento,
                            new EstornarPagamento(saga.OrdemServicoId, saga.CorrelationId, motivo));
                    }
                    break;
            }

            await AplicarDesfechoNaOrdemServicoAsync(saga, etapa, motivo, cancellationToken);

            SagaMetrics.SagaCompensada(etapa.ToString(), (agora - saga.DataCriacao).TotalMilliseconds);

            logger.LogWarning(
                "Saga da ordem de servico {OrdemServicoId} compensada na etapa {Etapa}: {Motivo}",
                saga.OrdemServicoId, etapa, motivo);

            return true;
        }

        private async Task AplicarDesfechoNaOrdemServicoAsync(
            SagaOrdemServico saga,
            SagaEtapa etapa,
            string motivo,
            CancellationToken cancellationToken)
        {
            var ordemServico = await ordemServicoRepository.ObterPorIdAsync(saga.OrdemServicoId, cancellationToken);

            if (ordemServico is null) return;

            // A falha ao gerar o orcamento nao cancela a OS: ela volta para o diagnostico e o
            // atendente pode tentar de novo. As demais falhas encerram a OS como Cancelada.
            if (etapa == SagaEtapa.Orcamento)
            {
                if (!ordemServico.PodeRetornarParaDiagnostico()) return;

                ordemServico.RetornarParaDiagnostico(motivo, saga.CorrelationId);
            }
            else
            {
                if (!ordemServico.PodeSerCancelada()) return;

                ordemServico.Cancelar(motivo, saga.CorrelationId);
            }

            await ordemServicoRepository.Atualizar(ordemServico);
        }

        private bool ConcluirCompensacao(SagaOrdemServico saga, DateTime agora)
        {
            if (!saga.AoEstornarPagamento(agora)) return false;

            logger.LogInformation(
                "Estorno confirmado; a compensacao da ordem de servico {OrdemServicoId} esta fechada.",
                saga.OrdemServicoId);

            return true;
        }

        private static TContrato Desserializar<TContrato>(MensagemRecebida mensagem) where TContrato : ISagaMensagem =>
            JsonSerializer.Deserialize<TContrato>(mensagem.Corpo, Json)
            ?? throw new InvalidOperationException($"Nao foi possivel ler o corpo da mensagem {mensagem.Tipo}.");
    }
}
