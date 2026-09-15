using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wrench.auto.repair.cadastro.application.Commands;
using wrench.auto.repair.core.Mediator;
using wrench.auto.repair.core.Pagination;
using wrench.auto.repair.ordem.servico.application.Paginacao;
using wrench.auto.repair.ordem.servico.application.Queries;
using wrench.auto.repair.ordem.servico.application.Queries.ViewModels;
using wrench.auto.repair.ordem.servico.application.UseCases.OrdemServicoUseCase;
using wrench.web.api.Extensions;
using wrench.web.api.Models.OrdemServico;

namespace wrench.web.api.Controllers
{
    /// <summary>
    /// Serviço para criar, atualizar e listar ordem de serviço
    /// </summary>
    [ApiVersion(1.0)]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Funcionario")]
    public class OrdemServicoController(IMediatorHandler _mediatorHandler) : ControllerBase
    {
        /// <summary>
        /// Cria ordem de serviço
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CriarOrdemServicoRequest request)
        {
            var result = await _mediatorHandler
                .EnviarComando<CriarOrdemServicoCommand, Guid>((CriarOrdemServicoCommand)request);

            return result.ToActionResult();
        }

        /// <summary>
        /// Cria ordem de serviço com cliente e veiculo
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("servico")]
        public async Task<IActionResult> PostServico([FromBody] NovaOrdemServicoRequest request)
        {
            var resultCliente = await _mediatorHandler
                .EnviarComando<CadastrarClienteCommand, Guid>(request.Cliente);

            if(!resultCliente.Sucesso)
            {
                return resultCliente.ToActionResult();
            }

            request.Veiculo.ClienteId = resultCliente.Valor;

            var resultVeiculo = await _mediatorHandler
                .EnviarComando<CadastrarVeiculoCommand, Guid>(request.Veiculo);

            if (!resultVeiculo.Sucesso)
            {
                return resultVeiculo.ToActionResult();
            }

            var result = await _mediatorHandler
               .EnviarComando<CriarOrdemServicoCommand, Guid>(new CriarOrdemServicoCommand(resultCliente.Valor, resultVeiculo.Valor, request.Descricao));

            return result.ToActionResult();
        }

        /// <summary>
        /// Finaliza ordem de serviço
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("{id:guid}/finalizar")]
        public async Task<IActionResult> FinalizarOrdem([FromRoute] Guid id)
        {
            var finalizarOrdemServicoCommand = new FinalizarOrdemServicoCommand(id);

            var result = await _mediatorHandler
                .EnviarComando<FinalizarOrdemServicoCommand>(finalizarOrdemServicoCommand);

            return result.ToActionResult();
        }

        /// <summary>
        /// Entregar Ordem de Serviço
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("{id:guid}/entregar")]
        public async Task<IActionResult> EntregarOrdemDeServico([FromRoute] Guid id)
        {
            var entregarServicoCommand = new EntregarServicoCommand(id);

            var result = await _mediatorHandler
                .EnviarComando<EntregarServicoCommand>(entregarServicoCommand);

            return result.ToActionResult();
        }

        /// <summary>
        /// Consultar ordem de serviço
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            var result = await _mediatorHandler
                .EnviarComando<ObterOrdemServicoIdQuery, OrdemServicoViewModel>(new ObterOrdemServicoIdQuery(id));

            return result.ToActionResult();
        }

        /// <summary>
        /// Listar todas as ordem de serviço
        /// </summary>
        /// <param name="requisicao"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] OrdemServicoRequisicaoPaginada requisicao)
        {
            var obterTodasOrdemServicoQuery = new ObterTodasOrdemServicoQuery(requisicao);

            var result = await _mediatorHandler
                .EnviarComando<ObterTodasOrdemServicoQuery, ResultadoPaginado<OrdemServicoViewModel>>(obterTodasOrdemServicoQuery);

            return result.ToActionResult();
        }

        /// <summary>
        /// Listar todas as ordens de um cliente
        /// </summary>
        /// <param name="clienteId"></param>
        /// <param name="veiculoId"></param>
        /// <param name="requisicao"></param>
        /// <returns></returns>
        [HttpGet("cliente")]
        [Authorize(Roles = "Admin,Funcionario")]
        public async Task<IActionResult> GetAllByClienteId([FromQuery] Guid clienteId, [FromQuery] Guid? veiculoId, [FromQuery] OrdemServicoRequisicaoPaginada requisicao)
        {
            var obterTodasOrdemServicoPorClienteQuery = new ObterTodasOrdemServicoPorClienteQuery(clienteId, veiculoId, requisicao);

            var result = await _mediatorHandler
                .EnviarComando<ObterTodasOrdemServicoPorClienteQuery, ResultadoPaginado<OrdemServicoViewModel>>(obterTodasOrdemServicoPorClienteQuery);

            return result.ToActionResult();
        }

        [HttpGet("monitoramento")]
        [Authorize(Roles = "Admin,Funcionario")]
        public async Task<IActionResult> ObterTempoMedioExecucaoOrdemServico()
        {
            var obterTempoMedioExecucaoOrdemServicoQuery = new ObterTempoMedioExecucaoOrdemServicoQuery();

            var result = await _mediatorHandler
                .EnviarComando<ObterTempoMedioExecucaoOrdemServicoQuery, MonitoramentoViewModel>(obterTempoMedioExecucaoOrdemServicoQuery);

            return result.ToActionResult();
        }
    }
}
