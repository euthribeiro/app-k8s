using System.Reflection;
using MediatR;
using wrench.auto.repair.core.Errors;

namespace wrench.auto.repair.ordem.servico.application.Metrics
{
    /// <summary>
    /// Pipeline do MediatR que alimenta o contador <see cref="OrdemServicoMetrics.FalhasProcessamentoNome"/>.
    /// Conta um comando de ordem de serviço como falha de processamento quando o handler lança exceção
    /// (a exceção é relançada sem alteração) ou devolve um resultado com <see cref="TipoErroEnum.INESPERADO"/>.
    /// Falhas de negócio, como validação, recurso não encontrado e conflito de status, não são contadas.
    /// O MediatR aplica pipeline behaviors a todas as requisições do container, então o escopo é restringido
    /// aos tipos do namespace <see cref="NamespaceComandos"/>; consultas e requisições de outros contextos
    /// passam direto.
    /// </summary>
    public sealed class FalhaProcessamentoOrdemServicoBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        /// <summary>Namespace raiz dos casos de uso de escrita do contexto de ordem de serviço.</summary>
        public const string NamespaceComandos = "wrench.auto.repair.ordem.servico.application.UseCases";

        private static readonly bool PertenceAoContexto =
            typeof(TRequest).Namespace?.StartsWith(NamespaceComandos, StringComparison.Ordinal) == true;

        private static readonly PropertyInfo? PropriedadeTipoErro =
            typeof(TResponse).GetProperty(nameof(Result.TipoErro), typeof(TipoErroEnum?));

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!PertenceAoContexto)
                return await next(cancellationToken);

            TResponse response;

            try
            {
                response = await next(cancellationToken);
            }
            catch (Exception ex)
            {
                OrdemServicoMetrics.RegistrarFalhaProcessamento(typeof(TRequest).Name, ex.GetType().Name);
                throw;
            }

            if (EhFalhaInesperada(response))
                OrdemServicoMetrics.RegistrarFalhaProcessamento(typeof(TRequest).Name, nameof(TipoErroEnum.INESPERADO));

            return response;
        }

        private static bool EhFalhaInesperada(TResponse response)
        {
            return response is not null
                && PropriedadeTipoErro?.GetValue(response) is TipoErroEnum.INESPERADO;
        }
    }
}
