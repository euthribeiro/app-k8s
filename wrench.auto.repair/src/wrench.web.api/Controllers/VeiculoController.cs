using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wrench.auto.repair.cadastro.application.Commands;
using wrench.auto.repair.cadastro.application.Commands.ViewModels;
using wrench.auto.repair.cadastro.application.Paginacao;
using wrench.auto.repair.cadastro.application.Queries;
using wrench.auto.repair.cadastro.application.Queries.ViewModels;
using wrench.auto.repair.core.Mediator;
using wrench.auto.repair.core.Pagination;
using wrench.web.api.Extensions;

namespace wrench.web.api.Controllers
{
    /// <summary>
    /// Serviço para criar, atualizar e listar todos os veículos
    /// </summary>
    /// <param name="_mediatorHandler"></param>
    [ApiVersion(1.0)]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    [Authorize(Roles = "Admin,Funcionario")]
    public class VeiculoController(
        IMediatorHandler _mediatorHandler
    ) : ControllerBase
    {
        /// <summary>
        /// Lista todos os veículos de forma paginada
        /// </summary>
        /// <param name="requisicao"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] VeiculoRequisicaoPaginada requisicao)
        {
            var obterTodosVeiculosQuery = new ObterTodosVeiculosQuery(requisicao);

            var resultado = await _mediatorHandler
                .EnviarComando<ObterTodosVeiculosQuery, ResultadoPaginado<VeiculoViewModel>>(obterTodosVeiculosQuery);

            return resultado.ToActionResult();
        }

        /// <summary>
        /// Busca um determinado veículo pelo seu identificador
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var obterVeiculoPorIdQuery = new ObterVeiculoPorIdQuery(id);

            var resultado = await _mediatorHandler
                .EnviarComando<ObterVeiculoPorIdQuery, VeiculoViewModel>(obterVeiculoPorIdQuery);

            return resultado.ToActionResult();
        }

        /// <summary>
        /// Busca um determinado veículo pela sua placa
        /// </summary>
        /// <param name="placa"></param>
        /// <returns></returns>
        [HttpGet("{placa}")]
        public async Task<IActionResult> GetByPlate([FromRoute] string placa)
        {
            var obterVeiculoPorPlacaQuery = new ObterVeiculoPorPlacaQuery(placa);

            var resultado = await _mediatorHandler
                .EnviarComando<ObterVeiculoPorPlacaQuery, VeiculoViewModel>(obterVeiculoPorPlacaQuery);

            return resultado.ToActionResult();
        }

        /// <summary>
        /// Cadastra um veiculo no sistema
        /// </summary>
        /// <param name="requisicao"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CadastrarVeiculo([FromBody] CadastrarVeiculoViewModel requisicao)
        {
            var cadastrarVeiculoCommand = new CadastrarVeiculoCommand
            (
                requisicao.ClienteId, requisicao.Marca,
                requisicao.Modelo, requisicao.Cor,
                requisicao.AnoFabricacao, requisicao.AnoModelo,
                requisicao.PlacaDoVeiculo, requisicao.Descricao,
                requisicao.UltimaRevisao, requisicao.QuilometragemAtual
            );

            var result = await _mediatorHandler
                .EnviarComando<CadastrarVeiculoCommand, Guid>(cadastrarVeiculoCommand);

            return result.ToActionResult();
        }

        /// <summary>
        /// Atualiza um veiculo no sistema
        /// </summary>
        /// <param name="requisicao"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> AtualizarVeiculo([FromBody] AtualizarVeiculoViewModel requisicao)
        {
            var atualizarVeiculoCommand = new AtualizarVeiculoCommand
            (
                requisicao.VeiculoId, requisicao.ClienteId, requisicao.Marca,
                requisicao.Modelo, requisicao.Cor, requisicao.AnoFabricacao,
                requisicao.AnoModelo, requisicao.PlacaDoVeiculo, requisicao.Descricao,
                requisicao.UltimaRevisao, requisicao.QuilometragemAtual
            );

            var result = await _mediatorHandler
                .EnviarComando<AtualizarVeiculoCommand>(atualizarVeiculoCommand);

            return result.ToActionResult();
        }
    }
}
