using Microsoft.AspNetCore.Mvc;
using Moq;
using wrench.auto.repair.cadastro.application.Commands;
using wrench.auto.repair.cadastro.application.Commands.ViewModels;
using wrench.auto.repair.core.Errors;
using wrench.auto.repair.core.Mediator;
using wrench.auto.repair.ordem.servico.application.UseCases.OrdemServicoUseCase;
using wrench.web.api.Controllers;
using wrench.web.api.Models.OrdemServico;

namespace wrench.web.api.tests;

public class OrdemServicoControllerTests
{
    private readonly Mock<IMediatorHandler> _mediatorMock = new();
    private readonly OrdemServicoController _controller;

    public OrdemServicoControllerTests()
    {
        _controller = new OrdemServicoController(_mediatorMock.Object);
    }

    [Fact]
    public async Task PostServico_DeveCriarClienteVeiculoEOrdemServico_QuandoRequisicaoForValida()
    {
        var clienteId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();
        var ordemServicoId = Guid.NewGuid();
        var clienteIdOriginalDoVeiculo = Guid.NewGuid();

        CadastrarClienteCommand? clienteCommand = null;
        CadastrarVeiculoCommand? veiculoCommand = null;
        CriarOrdemServicoCommand? ordemServicoCommand = null;

        var request = new NovaOrdemServicoRequest
        {
            Descricao = "Troca de freio",
            Cliente = new CadastrarClienteViewModel
            {
                Documento = "07940660039",
                Nome = "Cliente Teste",
                Telefone = "11999999999",
                Email = "thiago.r.ribeiro16@icloud.com",
                Endereco = new CriarEditarEnderecoViewModel
                {
                    Logradouro = "Rua Teste",
                    Numero = "123",
                    Complemento = "Apto 1",
                    Bairro = "Centro",
                    Cep = "01234-567",
                    Cidade = "São Paulo",
                    UnidadeFederativa = "SP",
                    Pais = "Brasil"
                }
            },
            Veiculo = new CadastrarVeiculoViewModel
            {
                ClienteId = clienteIdOriginalDoVeiculo,
                Marca = "Toyota",
                Modelo = "Corolla",
                Cor = "Preto",
                AnoFabricacao = 2020,
                AnoModelo = 2021,
                PlacaDoVeiculo = "ABC-1234",
                Descricao = "Veículo do cliente",
                UltimaRevisao = DateTime.UtcNow,
                QuilometragemAtual = 10000
            }
        };

        _mediatorMock
            .Setup(m => m.EnviarComando<CadastrarClienteCommand, Guid>(It.IsAny<CadastrarClienteCommand>()))
            .Callback<CadastrarClienteCommand>(command => clienteCommand = command)
            .ReturnsAsync(Result<Guid>.Created(clienteId));

        _mediatorMock
            .Setup(m => m.EnviarComando<CadastrarVeiculoCommand, Guid>(It.IsAny<CadastrarVeiculoCommand>()))
            .Callback<CadastrarVeiculoCommand>(command => veiculoCommand = command)
            .ReturnsAsync(Result<Guid>.Created(veiculoId));

        _mediatorMock
            .Setup(m => m.EnviarComando<CriarOrdemServicoCommand, Guid>(It.IsAny<CriarOrdemServicoCommand>()))
            .Callback<CriarOrdemServicoCommand>(command => ordemServicoCommand = command)
            .ReturnsAsync(Result<Guid>.Created(ordemServicoId));

        var result = await _controller.PostServico(request);

        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.Equal(ordemServicoId, createdResult.Value);

        Assert.NotNull(clienteCommand);
        Assert.Equal(request.Cliente.Documento, clienteCommand!.Documento);
        Assert.Equal(request.Cliente.Nome, clienteCommand.Nome);
        Assert.Equal(request.Cliente.Telefone, clienteCommand.Telefone);
        Assert.Equal(request.Cliente.Email, clienteCommand.Email);

        Assert.NotNull(veiculoCommand);
        Assert.Equal(clienteId, veiculoCommand!.ClienteId);
        Assert.Equal(request.Veiculo.Marca, veiculoCommand.Marca);
        Assert.Equal(request.Veiculo.Modelo, veiculoCommand.Modelo);
        Assert.Equal(request.Veiculo.Cor, veiculoCommand.Cor);
        Assert.Equal(request.Veiculo.AnoFabricacao, veiculoCommand.AnoFabricacao);
        Assert.Equal(request.Veiculo.AnoModelo, veiculoCommand.AnoModelo);
        Assert.Equal(request.Veiculo.PlacaDoVeiculo, veiculoCommand.PlacaDoVeiculo);
        Assert.Equal(request.Veiculo.Descricao, veiculoCommand.Descricao);
        Assert.Equal(request.Veiculo.UltimaRevisao, veiculoCommand.UltimaRevisao);
        Assert.Equal(request.Veiculo.QuilometragemAtual, veiculoCommand.QuilometragemAtual);

        Assert.NotNull(ordemServicoCommand);
        Assert.Equal(clienteId, ordemServicoCommand!.ClienteId);
        Assert.Equal(veiculoId, ordemServicoCommand.VeiculoId);
        Assert.Equal(request.Descricao, ordemServicoCommand.Descricao);

        _mediatorMock.Verify(m => m.EnviarComando<CadastrarClienteCommand, Guid>(It.IsAny<CadastrarClienteCommand>()), Times.Once);
        _mediatorMock.Verify(m => m.EnviarComando<CadastrarVeiculoCommand, Guid>(It.IsAny<CadastrarVeiculoCommand>()), Times.Once);
        _mediatorMock.Verify(m => m.EnviarComando<CriarOrdemServicoCommand, Guid>(It.IsAny<CriarOrdemServicoCommand>()), Times.Once);
    }
}
