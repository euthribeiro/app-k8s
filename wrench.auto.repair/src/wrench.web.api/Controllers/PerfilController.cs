using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wrench.auto.repair.autenticacao.application.Queries;
using wrench.auto.repair.autenticacao.application.Queries.ViewModels;
using wrench.auto.repair.core.Mediator;
using wrench.web.api.Extensions;

namespace wrench.web.api.Controllers
{
    /// <summary>
    /// Serviço para Listar Perfis
    /// </summary>
    [ApiVersion(1.0)]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class PerfilController(IMediatorHandler _mediatorHandler) : ControllerBase
    {
        /// <summary>
        /// Listar todos os perfis
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _mediatorHandler
                .EnviarComando<ObterTodosPerfisQuery, IEnumerable<PerfilViewModel>>(
                    new ObterTodosPerfisQuery()
                );

            return result.ToActionResult();
        }
    }
}
