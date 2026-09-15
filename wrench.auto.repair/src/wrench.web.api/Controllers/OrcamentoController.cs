using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wrench.auto.repair.core.Mediator;
using wrench.auto.repair.ordem.servico.application.UseCases.OrcamentoUseCase;
using wrench.web.api.Extensions;
using wrench.web.api.Models.Orcamento;

namespace wrench.web.api.Controllers
{
    /// <summary>
    /// Serviço para aprovar e listar orçamentos
    /// </summary>
    [ApiVersion(1.0)]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Cliente")]
    public class OrcamentoController(IMediatorHandler _mediatorHandler) : ControllerBase
    {
        /// <summary>
        /// Aprovar orçamento
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{id:guid}/aprovar")]
        public async Task<IActionResult> AprovarOrcamento([FromRoute] Guid id)
        {
            var aprovarOrcamentoCommand = new AprovaOrcamentoCommand(id);

            var result = await _mediatorHandler
                .EnviarComando<AprovaOrcamentoCommand>(aprovarOrcamentoCommand);

            return result.ToActionResult();
        }

        /// <summary>
        /// Recusar orçamento
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{id:guid}/recusar")]
        public async Task<IActionResult> RecusarOrcamento([FromRoute] Guid id, [FromBody] RecusarOrcamentoRequest request)
        {
            var recusarOrcamento = new RecusarOrcamentoCommand(id, request.MotivoRecusa);

            var result = await _mediatorHandler
                .EnviarComando<RecusarOrcamentoCommand>(recusarOrcamento);

            return result.ToActionResult();
        }
    }
}
