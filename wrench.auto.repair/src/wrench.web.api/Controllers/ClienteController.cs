using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wrench.auto.repair.cadastro.application.Commands;
using wrench.auto.repair.cadastro.application.Commands.Dtos;
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
    /// Serviço para criar, atualizar e listar clientes
    /// </summary>
    /// <param name="_mediatorHandler"></param>
    [ApiVersion(1.0)]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Funcionario")]
    public class ClienteController(
        IMediatorHandler _mediatorHandler
    ) : ControllerBase
    {
        /// <summary>
        /// Lista todos os clientes de forma paginada
        /// </summary>
        /// <param name="requisicao"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ClienteRequisicaoPaginada requisicao)
        {
            var obterTodosClientesQuery = new ObterTodosClientesQuery(requisicao);

            var result = await _mediatorHandler
                .EnviarComando<ObterTodosClientesQuery, ResultadoPaginado<ClienteViewModel>>(obterTodosClientesQuery);

            return result.ToActionResult();
        }

        /// <summary>
        /// Busca um determinado cliente pelo seu identificar
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var obterClientePorIdQuery = new ObterClientePorIdQuery(id);

            var result = await _mediatorHandler
                .EnviarComando<ObterClientePorIdQuery, ClienteViewModel>(obterClientePorIdQuery);

            return result.ToActionResult();
        }

        /// <summary>
        /// Busca um determinado cliente pelo seu documento (CPF/CNPJ)
        /// </summary>
        /// <param name="documento"></param>
        /// <returns></returns>
        [HttpGet("{documento}")]
        public async Task<IActionResult> GetByDocument([FromRoute] string documento)
        {
            var obterClientePorDocumentoQuery = new ObterClientePorDocumentoQuery(documento);

            var result = await _mediatorHandler
                .EnviarComando<ObterClientePorDocumentoQuery, ClienteViewModel>(obterClientePorDocumentoQuery);

            return result.ToActionResult();
        }

        /// <summary>
        /// Cadastra um cliente no sistema
        /// </summary>
        /// <param name="requisicao"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CadastrarCliente([FromBody] CadastrarClienteViewModel requisicao)
        {
            var endereco = new EnderecoDto(
                requisicao.Endereco.Logradouro,
                requisicao.Endereco.Numero,
                requisicao.Endereco.Complemento,
                requisicao.Endereco.Bairro,
                requisicao.Endereco.Cep,
                requisicao.Endereco.Cidade,
                requisicao.Endereco.UnidadeFederativa
            );

            var cadastrarClienteCommand = new CadastrarClienteCommand(
                requisicao.Documento, requisicao.Nome, requisicao.Telefone,
                requisicao.Email, endereco
            );

            var result = await _mediatorHandler
                .EnviarComando<CadastrarClienteCommand, Guid>(cadastrarClienteCommand);

            return result.ToActionResult();
        }

        /// <summary>
        /// Atualiza um cliente no sistema
        /// </summary>
        /// <param name="requisicao"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> AtualizarCliente([FromBody] AtualizarClienteViewModel requisicao)
        {
            EnderecoDto? endereco = null;

            if (requisicao.Endereco != null)
            {
                endereco = new EnderecoDto(
                    requisicao.Endereco.Logradouro,
                    requisicao.Endereco.Numero,
                    requisicao.Endereco.Complemento,
                    requisicao.Endereco.Bairro,
                    requisicao.Endereco.Cep,
                    requisicao.Endereco.Cidade,
                    requisicao.Endereco.UnidadeFederativa
                );
            }

            var atualizarClienteCommand =
                new AtualizarClienteCommand(requisicao.ClienteId, requisicao.Nome,
                                            requisicao.Telefone, requisicao.Email, endereco);

            var result = await _mediatorHandler
                .EnviarComando<AtualizarClienteCommand>(atualizarClienteCommand);

            return result.ToActionResult();
        }
    }
}
