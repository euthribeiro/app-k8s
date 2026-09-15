using Moq;
using Moq.AutoMock;
using wrench.auto.repair.cadastro.application.Commands;
using wrench.auto.repair.cadastro.application.tests.Fixture;
using wrench.auto.repair.cadastro.domain.Data;
using wrench.auto.repair.cadastro.domain.Entities;

namespace wrench.auto.repair.cadastro.application.tests.VeiculoCommand
{
    [Collection(nameof(VeiculoCollection))]
    public class VeiculoCommandHandlerTests(
        VeiculoFixture _veiculoFixture,
        ClienteFixture _clienteFixture
    ) : IClassFixture<VeiculoFixture>,
        IClassFixture<ClienteFixture>
    {
        [Fact(DisplayName = "Cadastrar Novo Veiculo Com Sucesso")]
        [Trait("Cadastro", "Application")]
        public async Task Veiculo_CadastrarVeiclo_DeveCadastrarComSucesso()
        {
            var cliente = _clienteFixture.GerarClienteValido();
            var marca = "Fiat";
            var modelo = "Uno";
            var anoFabricacao = 2018;
            var anoModelo = 2018;
            var cor = _veiculoFixture.GerarCorAleatoria();
            var placaVeiculo = _veiculoFixture.GerarPlacaVeiculoValida();
            var cadastrarVeiculoCommand =
                new CadastrarVeiculoCommand(cliente.Id, marca, modelo, cor, anoFabricacao, anoModelo, placaVeiculo, null, DateTime.UtcNow, 0);
            var automocker = new AutoMocker();
            var veiculoCommandHandler = automocker.CreateInstance<VeiculoCommandHandler>();

            automocker.GetMock<IVeiculoRepository>()
                .Setup(c => c.ObterVeiculoPelaPlacaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Veiculo?>(null));

            automocker.GetMock<IClienteRepository>()
             .Setup(c => c.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .Returns(Task.FromResult<Cliente?>(cliente));

            automocker.GetMock<IVeiculoRepository>()
                 .Setup(c => c.UnitOfWork.CommitAsync())
                 .Returns(Task.FromResult<bool>(true));

            var result = await veiculoCommandHandler
                .Handle(cadastrarVeiculoCommand, CancellationToken.None);

            Assert.True(result.Sucesso);

            automocker.GetMock<IVeiculoRepository>()
                .Verify(c => c.ObterVeiculoPelaPlacaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IClienteRepository>()
                .Verify(c => c.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IVeiculoRepository>()
                .Verify(c => c.Adicionar(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IVeiculoRepository>()
                .Verify(c => c.UnitOfWork.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "Cadastrar Veiculo Placa Existente Não Deve Cadastrar")]
        [Trait("Cadastro", "Application")]
        public async Task CadastrarVeiculo_PlacaVeiculoExistente_NaoDeveCadastrar()
        {
            var veiculoJaCadastrado = _veiculoFixture.CriarVeiculoValido();
            var cliente = _clienteFixture.GerarClienteValido();
            var marca = "Fiat";
            var modelo = "Uno";
            var anoFabricacao = 2018;
            var anoModelo = 2018;
            var cor = _veiculoFixture.GerarCorAleatoria();
            var placaVeiculo = _veiculoFixture.GerarPlacaVeiculoValida();
            var cadastrarVeiculoCommand =
                new CadastrarVeiculoCommand(cliente.Id, marca, modelo, cor, anoFabricacao, anoModelo, placaVeiculo, null, DateTime.UtcNow, 0);
            var automocker = new AutoMocker();
            var veiculoCommandHandler = automocker.CreateInstance<VeiculoCommandHandler>();

            automocker.GetMock<IVeiculoRepository>()
                .Setup(c => c.ObterVeiculoPelaPlacaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Veiculo?>(veiculoJaCadastrado));

            automocker.GetMock<IClienteRepository>()
                .Verify(c => c.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);

            automocker.GetMock<IClienteRepository>()
                .Setup(c => c.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Cliente?>(cliente));

            automocker.GetMock<IVeiculoRepository>()
                .Setup(c => c.UnitOfWork.CommitAsync())
                .Returns(Task.FromResult<bool>(true));

            var result = await veiculoCommandHandler
                .Handle(cadastrarVeiculoCommand, CancellationToken.None);

            Assert.False(result.Sucesso);

            automocker.GetMock<IVeiculoRepository>()
                .Verify(c => c.ObterVeiculoPelaPlacaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IClienteRepository>()
                .Verify(c => c.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);

            automocker.GetMock<IVeiculoRepository>()
                .Verify(c => c.Adicionar(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()), Times.Never);

            automocker.GetMock<IVeiculoRepository>()
                .Verify(c => c.UnitOfWork.CommitAsync(), Times.Never);
        }

        [Fact(DisplayName = "Cadastrar Veiculo Placa Existente Não Deve Cadastrar")]
        [Trait("Cadastro", "Application")]
        public async Task CadastrarVeiculo_ClienteNaoEncontrado_NaoDeveCadastrar()
        {
            var cliente = _clienteFixture.GerarClienteValido();
            var marca = "Fiat";
            var modelo = "Uno";
            var anoFabricacao = 2018;
            var anoModelo = 2018;
            var cor = _veiculoFixture.GerarCorAleatoria();
            var placaVeiculo = _veiculoFixture.GerarPlacaVeiculoValida();
            var cadastrarVeiculoCommand =
                new CadastrarVeiculoCommand(cliente.Id, marca, modelo, cor, anoFabricacao, anoModelo, placaVeiculo, null, DateTime.UtcNow, 0);
            var automocker = new AutoMocker();
            var veiculoCommandHandler = automocker.CreateInstance<VeiculoCommandHandler>();

            automocker.GetMock<IVeiculoRepository>()
                .Setup(c => c.ObterVeiculoPelaPlacaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Veiculo?>(null));

            automocker.GetMock<IClienteRepository>()
                .Verify(c => c.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);

            automocker.GetMock<IClienteRepository>()
                .Setup(c => c.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Cliente?>(null));

            automocker.GetMock<IVeiculoRepository>()
                .Setup(c => c.UnitOfWork.CommitAsync())
                .Returns(Task.FromResult<bool>(true));

            var result = await veiculoCommandHandler
                .Handle(cadastrarVeiculoCommand, CancellationToken.None);

            Assert.False(result.Sucesso);

            automocker.GetMock<IVeiculoRepository>()
                .Verify(c => c.ObterVeiculoPelaPlacaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IClienteRepository>()
                .Verify(c => c.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IVeiculoRepository>()
                .Verify(c => c.Adicionar(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()), Times.Never);

            automocker.GetMock<IVeiculoRepository>()
                .Verify(c => c.UnitOfWork.CommitAsync(), Times.Never);
        }

        [Fact(DisplayName = "Atualizar Veiculo Com Sucesso")]
        [Trait("Cadastro", "Application")]
        public async Task Cliente_AtualizarVeiculo_DeveAtualizarComSucesso()
        {
            var cliente = _clienteFixture.GerarClienteValido();
            var veiculoEncontrado = _veiculoFixture.CriarVeiculoValido();
            var marca = "Fiat";
            var modelo = "Uno";
            var anoFabricacao = 2018;
            var anoModelo = 2018;
            var cor = _veiculoFixture.GerarCorAleatoria();
            var placaVeiculo = _veiculoFixture.GerarPlacaVeiculoValida();
            var atualizarVeiculoCommand =
                new AtualizarVeiculoCommand(veiculoEncontrado.Id, cliente.Id, marca, modelo, cor, anoFabricacao, anoModelo, placaVeiculo, null, DateTime.UtcNow, 0);
            var automocker = new AutoMocker();
            var veiculoCommandHandler = automocker.CreateInstance<VeiculoCommandHandler>();

            automocker.GetMock<IVeiculoRepository>()
                .Setup(c => c.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Veiculo?>(veiculoEncontrado));

            automocker.GetMock<IVeiculoRepository>()
             .Setup(c => c.ObterVeiculoPelaPlacaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .Returns(Task.FromResult<Veiculo?>(null));

            automocker.GetMock<IClienteRepository>()
             .Setup(c => c.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .Returns(Task.FromResult<Cliente?>(cliente));

            automocker.GetMock<IVeiculoRepository>()
                 .Setup(c => c.UnitOfWork.CommitAsync())
                 .Returns(Task.FromResult<bool>(true));

            var result = await veiculoCommandHandler
                .Handle(atualizarVeiculoCommand, CancellationToken.None);

            Assert.True(result.Sucesso);

            automocker.GetMock<IVeiculoRepository>()
                .Verify(c => c.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IVeiculoRepository>()
                .Verify(c => c.ObterVeiculoPelaPlacaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IClienteRepository>()
                .Verify(c => c.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IVeiculoRepository>()
                .Verify(c => c.Atualizar(It.IsAny<Veiculo>()), Times.Once);

            automocker.GetMock<IVeiculoRepository>()
                .Verify(c => c.UnitOfWork.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "Atualizar Veiculo Inexistente")]
        [Trait("Cadastro", "Application")]
        public async Task AtualizarVeiculo_VeiculoInexistente_DeveRetornarErro()
        {
            var cliente = _clienteFixture.GerarClienteValido();
            var veiculoId = Guid.NewGuid();
            var marca = "Fiat";
            var modelo = "Uno";
            var anoFabricacao = 2018;
            var anoModelo = 2018;
            var cor = _veiculoFixture.GerarCorAleatoria();
            var placaVeiculo = _veiculoFixture.GerarPlacaVeiculoValida();
            var atualizarVeiculoCommand =
                new AtualizarVeiculoCommand(veiculoId, cliente.Id, marca, modelo, cor, anoFabricacao, anoModelo, placaVeiculo, null, DateTime.UtcNow, 0);
            var automocker = new AutoMocker();
            var veiculoCommandHandler = automocker.CreateInstance<VeiculoCommandHandler>();

            automocker.GetMock<IVeiculoRepository>()
                .Setup(c => c.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Veiculo?>(null));

            automocker.GetMock<IVeiculoRepository>()
             .Setup(c => c.ObterVeiculoPelaPlacaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .Returns(Task.FromResult<Veiculo?>(null));

            automocker.GetMock<IClienteRepository>()
             .Setup(c => c.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .Returns(Task.FromResult<Cliente?>(cliente));

            automocker.GetMock<IVeiculoRepository>()
                 .Setup(c => c.UnitOfWork.CommitAsync())
                 .Returns(Task.FromResult<bool>(true));

            var result = await veiculoCommandHandler
                .Handle(atualizarVeiculoCommand, CancellationToken.None);

            Assert.False(result.Sucesso);

            automocker.GetMock<IVeiculoRepository>()
                .Verify(c => c.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IVeiculoRepository>()
                .Verify(c => c.ObterVeiculoPelaPlacaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

            automocker.GetMock<IClienteRepository>()
                .Verify(c => c.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);

            automocker.GetMock<IVeiculoRepository>()
                .Verify(c => c.Atualizar(It.IsAny<Veiculo>()), Times.Never);

            automocker.GetMock<IVeiculoRepository>()
                .Verify(c => c.UnitOfWork.CommitAsync(), Times.Never);
        }

        [Fact(DisplayName = "Atualizar Veiculo Placa Já Cadastrada")]
        [Trait("Cadastro", "Application")]
        public async Task AtualizarVeiculo_PlacaJaCadastrada_DeveRetornarErro()
        {
            var cliente = _clienteFixture.GerarClienteValido();
            var veiculoEncontrado = _veiculoFixture.CriarVeiculoValido();
            var veiculoMesmaPlaca = veiculoEncontrado;
            var marca = "Fiat";
            var modelo = "Uno";
            var anoFabricacao = 2018;
            var anoModelo = 2018;
            var cor = _veiculoFixture.GerarCorAleatoria();
            var placaVeiculo = _veiculoFixture.GerarPlacaVeiculoValida();
            var atualizarVeiculoCommand =
                new AtualizarVeiculoCommand(veiculoEncontrado.Id, cliente.Id, marca, modelo, cor, anoFabricacao, anoModelo, placaVeiculo, null, DateTime.UtcNow, 0);
            var automocker = new AutoMocker();
            var veiculoCommandHandler = automocker.CreateInstance<VeiculoCommandHandler>();

            automocker.GetMock<IVeiculoRepository>()
                .Setup(c => c.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Veiculo?>(veiculoEncontrado));

            automocker.GetMock<IVeiculoRepository>()
             .Setup(c => c.ObterVeiculoPelaPlacaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .Returns(Task.FromResult<Veiculo?>(veiculoMesmaPlaca));

            automocker.GetMock<IClienteRepository>()
             .Setup(c => c.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .Returns(Task.FromResult<Cliente?>(cliente));

            automocker.GetMock<IVeiculoRepository>()
                 .Setup(c => c.UnitOfWork.CommitAsync())
                 .Returns(Task.FromResult<bool>(true));

            var result = await veiculoCommandHandler
                .Handle(atualizarVeiculoCommand, CancellationToken.None);

            Assert.False(result.Sucesso);

            automocker.GetMock<IVeiculoRepository>()
                .Verify(c => c.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IVeiculoRepository>()
                .Verify(c => c.ObterVeiculoPelaPlacaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IClienteRepository>()
                .Verify(c => c.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IVeiculoRepository>()
                .Verify(c => c.Atualizar(It.IsAny<Veiculo>()), Times.Never);

            automocker.GetMock<IVeiculoRepository>()
                .Verify(c => c.UnitOfWork.CommitAsync(), Times.Never);
        }

        [Fact(DisplayName = "Atualizar Veiculo Cliente Não Encontrado")]
        [Trait("Cadastro", "Application")]
        public async Task AtualizarVeiculo_ClienteNaoEncontrado_DeveRetornarErro()
        {
            var cliente = _clienteFixture.GerarClienteValido();
            var veiculoEncontrado = _veiculoFixture.CriarVeiculoValido();
            var marca = "Fiat";
            var modelo = "Uno";
            var anoFabricacao = 2018;
            var anoModelo = 2018;
            var cor = _veiculoFixture.GerarCorAleatoria();
            var placaVeiculo = _veiculoFixture.GerarPlacaVeiculoValida();
            var atualizarVeiculoCommand =
                new AtualizarVeiculoCommand(veiculoEncontrado.Id, cliente.Id, marca, modelo, cor, anoFabricacao, anoModelo, placaVeiculo, null, DateTime.UtcNow, 0);
            var automocker = new AutoMocker();
            var veiculoCommandHandler = automocker.CreateInstance<VeiculoCommandHandler>();

            automocker.GetMock<IVeiculoRepository>()
                .Setup(c => c.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Veiculo?>(veiculoEncontrado));

            automocker.GetMock<IVeiculoRepository>()
             .Setup(c => c.ObterVeiculoPelaPlacaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .Returns(Task.FromResult<Veiculo?>(null));

            automocker.GetMock<IClienteRepository>()
             .Setup(c => c.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .Returns(Task.FromResult<Cliente?>(null));

            automocker.GetMock<IVeiculoRepository>()
                 .Setup(c => c.UnitOfWork.CommitAsync())
                 .Returns(Task.FromResult<bool>(true));

            var result = await veiculoCommandHandler
                .Handle(atualizarVeiculoCommand, CancellationToken.None);

            Assert.False(result.Sucesso);

            automocker.GetMock<IVeiculoRepository>()
                .Verify(c => c.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IVeiculoRepository>()
                .Verify(c => c.ObterVeiculoPelaPlacaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

            automocker.GetMock<IClienteRepository>()
                .Verify(c => c.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IVeiculoRepository>()
                .Verify(c => c.Atualizar(It.IsAny<Veiculo>()), Times.Never);

            automocker.GetMock<IVeiculoRepository>()
                .Verify(c => c.UnitOfWork.CommitAsync(), Times.Never);
        }
    }
}
