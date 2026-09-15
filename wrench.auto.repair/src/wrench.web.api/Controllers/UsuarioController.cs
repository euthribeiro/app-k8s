using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wrench.auto.repair.autenticacao.application.Commands;
using wrench.auto.repair.autenticacao.application.Commands.ViewModels;
using wrench.auto.repair.autenticacao.application.Paginacao;
using wrench.auto.repair.autenticacao.application.Queries;
using wrench.auto.repair.autenticacao.application.Queries.ViewModels;
using wrench.auto.repair.core.Mediator;
using wrench.auto.repair.core.Pagination;
using wrench.web.api.Extensions;

namespace wrench.web.api.Controllers
{
    /// <summary>
    /// Serviço para Cadastrar, Atualizar, (In)ativar e Listar Usuários
    /// </summary>
    /// <param name="_mediatorHandler"></param>
    [ApiVersion(1.0)]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsuarioController(IMediatorHandler _mediatorHandler) : ControllerBase
    {
        /// <summary>
        /// Criar um novo usuário
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CriarUsuarioViewModel request)
        {
            var command = new CriarUsuarioCommand(request.Email, request.Senha, request.PerfilId, request.Ativo);

            var result = await _mediatorHandler.EnviarComando<CriarUsuarioCommand, Guid>(command);

            return result.ToActionResult();
        }

        /// <summary>
        /// Lista todos os usuários cadastrados
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] UsuarioRequisicaoPaginada request)
        {
            var obterTodosUsuariosQuery = new ObterTodosUsuariosQuery(request);

            var result = await _mediatorHandler
                .EnviarComando<ObterTodosUsuariosQuery, ResultadoPaginado<UsuarioViewModel>>(obterTodosUsuariosQuery);

            return result.ToActionResult();
        }

        /// <summary>
        /// Busca usuário pelo id
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var obterUsuarioPorIdQuery = new ObterUsuarioPorIdQuery(id);

            var result = await _mediatorHandler
                .EnviarComando<ObterUsuarioPorIdQuery, UsuarioViewModel>(obterUsuarioPorIdQuery);

            return result.ToActionResult();
        }

        /// <summary>
        /// Ativar o acesso do usuário
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("{id:guid}/ativar")]
        public async Task<IActionResult> AtivarUsuarioAsync([FromRoute] Guid id)
        {
            var ativarUsuarioCommand = new AtivarUsuarioCommand(id);

            var resultado = await _mediatorHandler
               .EnviarComando<AtivarUsuarioCommand>(ativarUsuarioCommand);

            return resultado.ToActionResult();
        }

        /// <summary>
        /// Inativar um usuário
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
        {
            var inativarUsuarioCommand = new InativarUsuarioCommand(id);

            var resultado = await _mediatorHandler
                .EnviarComando<InativarUsuarioCommand>(inativarUsuarioCommand);

            return resultado.ToActionResult();
        }

        /// <summary>
        /// Realizar primeiro acesso
        /// </summary>
        /// <param name="requisicao"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPut("primeiro-acesso")]
        public async Task<IActionResult> PrimeiroAcessoSenhaAsync([FromBody] PrimeirAcessoUsuarioViewModel requisicao)
        {
            var primeiroAcessoUsuarioCommand =
                new PrimeiroAcessoUsuarioCommand(requisicao.Email, requisicao.Senha);

            var resultado = await _mediatorHandler
                .EnviarComando<PrimeiroAcessoUsuarioCommand>(primeiroAcessoUsuarioCommand);

            return resultado.ToActionResult();
        }

        /// <summary>
        /// Reseta o acesso do usuário
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("{id:guid}/resetar-acesso")]
        public async Task<IActionResult> ResetarAcessoAsync([FromRoute] Guid id)
        {
            var alterarSenhaUsuarioCommand = new ResetarSenhaUsuarioCommand(id);

            var resultado = await _mediatorHandler
               .EnviarComando<ResetarSenhaUsuarioCommand>(alterarSenhaUsuarioCommand);

            return resultado.ToActionResult();
        }
    }
}
