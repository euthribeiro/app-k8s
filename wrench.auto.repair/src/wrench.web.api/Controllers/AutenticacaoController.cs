using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wrench.auto.repair.autenticacao.application.Commands;
using wrench.auto.repair.autenticacao.application.Commands.ViewModels;
using wrench.auto.repair.autenticacao.domain.Models;
using wrench.auto.repair.core.Mediator;
using wrench.web.api.Extensions;

namespace wrench.web.api.Controllers
{
    /// <summary>
    /// Autenticar Usuário
    /// </summary>
    /// <param name="_mediatorHandler"></param>
    [ApiVersion(1.0)]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class AutenticacaoController(IMediatorHandler _mediatorHandler) : ControllerBase
    {
        /// <summary>
        /// Autenticar Usuário para utilizar a API
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Autenticar([FromBody] AutenticarUsuarioViewModel request)
        {
            var autenticarUsuarioCommand = new AutenticarUsuarioCommand(request.Email, request.Senha);

            var result = await _mediatorHandler
                .EnviarComando<AutenticarUsuarioCommand, TokenAcesso>(autenticarUsuarioCommand);

            return result.ToActionResult();
        }
    }
}
