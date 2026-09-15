using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wrench.auto.repair.core.Mediator;
using wrench.auto.repair.core.Pagination;
using wrench.auto.repair.estoque.application.Commands;
using wrench.auto.repair.estoque.application.Paginacao;
using wrench.auto.repair.estoque.application.Queries;
using wrench.auto.repair.estoque.application.Queries.ViewModels;
using wrench.web.api.Extensions;
using wrench.web.api.Models.Requests;

namespace wrench.web.api.Controllers
{
    /// <summary>
    /// Serviço para criar, atualizar e listar peças e insumos
    /// </summary>
    [ApiVersion(1.0)]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Funcionario")]
    public class PecaController(IMediatorHandler _mediatorHandler) : ControllerBase
    {
        /// <summary>
        /// Busca peças e insumos por Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns name="peca"></returns>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPecaPorId([FromRoute] Guid id)
        {
            var consultaPecaPorIdQuery = new ConsultarPecaPorIdQuery(id);

            var result = await _mediatorHandler
                .EnviarComando<ConsultarPecaPorIdQuery, PecaViewModel>(consultaPecaPorIdQuery);

            return result.ToActionResult();
        }

        /// <summary>
        /// Lista todas as peças e insumos de forma paginada
        /// </summary>
        /// <param name="requisicao"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PecaRequisicaoPaginada requisicao)
        {
            var obterTodasPecasQuery = new ObterTodasPecasQuery(requisicao);

            var resultado = await _mediatorHandler
                .EnviarComando<ObterTodasPecasQuery, ResultadoPaginado<PecaViewModel>>(obterTodasPecasQuery);

            return resultado.ToActionResult();
        }

        /// <summary>
        /// Cria peças e insumos
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CriarPecaRequest request)
        {
            var cadastrarPecaCommand = (CadastrarPecaCommand)request;

            var resultado = await _mediatorHandler
                .EnviarComando<CadastrarPecaCommand, Guid>(cadastrarPecaCommand);

            return resultado.ToActionResult();
        }

        /// <summary>
        /// Atualiza informações da peça/insumo
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> AtualizarPeca([FromBody] AtualizarPecaRequest request)
        {
            var atualizarPecaCommand = new AtualizarPecaCommand(request.Id, request.Nome, request.Descricao, request.Valor, request.Ativo);

            var resultado = await _mediatorHandler
                .EnviarComando<AtualizarPecaCommand>(atualizarPecaCommand);

            return resultado.ToActionResult();
        }

        /// <summary>
        /// Repor peça/insumo no estoque
        /// </summary>
        /// <param name="id"></param>
        /// <param name="quantidade"></param>
        /// <returns></returns>
        [HttpPut("{id:guid}/repor")]
        public async Task<IActionResult> ReporEstoque([FromRoute] Guid id, [FromQuery] int quantidade)
        {
            var reporPecaCommand = new ReporPecaCommand(id, quantidade);

            var resultado = await _mediatorHandler
                .EnviarComando<ReporPecaCommand>(reporPecaCommand);

            return resultado.ToActionResult();
        }

        /// <summary>
        /// Baixar peça/insumo no estoque
        /// </summary>
        /// <param name="id"></param>
        /// <param name="quantidade"></param>
        /// <returns></returns>
        [HttpPut("{id:guid}/baixar")]
        public async Task<IActionResult> BaixarEstoque([FromRoute] Guid id, [FromQuery] int quantidade)
        {
            var baixarPecaCommand = new BaixarPecaCommand(id, quantidade);

            var resultado = await _mediatorHandler
                .EnviarComando<BaixarPecaCommand>(baixarPecaCommand);

            return resultado.ToActionResult();
        }

        /// <summary>
        /// Reativar peça/insumo
        /// </summary>
        /// <param name="id"></param>
        /// <param name="quantidade"></param>
        /// <returns></returns>
        [HttpPut("{id:guid}/ativar")]
        public async Task<IActionResult> ReativarPeca([FromRoute] Guid id)
        {
            var ativarPecaCommand = new AtivarPecaCommand(id);

            var resultado = await _mediatorHandler
                .EnviarComando<AtivarPecaCommand>(ativarPecaCommand);

            return resultado.ToActionResult();
        }

        /// <summary>
        /// Desativar peça/insumo
        /// </summary>
        /// <param name="id"></param>
        /// <param name="quantidade"></param>
        /// <returns></returns>
        [HttpPut("{id:guid}/desativar")]
        public async Task<IActionResult> DesativarPeca([FromRoute] Guid id)
        {
            var inativarPecaCommand = new InativarPecaCommand(id);

            var resultado = await _mediatorHandler
                .EnviarComando<InativarPecaCommand>(inativarPecaCommand);

            return resultado.ToActionResult();
        }
    }
}
