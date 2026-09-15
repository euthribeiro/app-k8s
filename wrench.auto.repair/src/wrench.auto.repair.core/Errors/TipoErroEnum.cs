namespace wrench.auto.repair.core.Errors
{
    /// <summary>
    /// Classifica a falha de um <see cref="Result"/> e determina o status HTTP
    /// devolvido pela API. A traducao para <c>IActionResult</c> e feita em
    /// <c>wrench.web.api.Extensions.ResultExtensions</c>.
    /// </summary>
    public enum TipoErroEnum
    {
        /// <summary>Requisicao bem formada, mas com conteudo que o dominio recusa. Responde 422 Unprocessable Entity.</summary>
        ENTIDADE_NAO_PROCESSAVEL,

        /// <summary>Falha de validacao de entrada. Responde 400 Bad Request.</summary>
        VALIDACAO,

        /// <summary>Recurso inexistente. Responde 404 Not Found.</summary>
        NAO_ENCONTRADO,

        /// <summary>Conflito com o estado atual do recurso. Responde 409 Conflict.</summary>
        CONFLITO,

        /// <summary>Falha nao prevista. Responde 500 Internal Server Error.</summary>
        INESPERADO,

        /// <summary>Credencial ausente ou invalida. Responde 401 Unauthorized.</summary>
        NAO_AUTORIZADO,

        /// <summary>Autenticado, porem sem permissao para a operacao. Responde 403 Forbidden.</summary>
        PROIBIDO
    }
}
