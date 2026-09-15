using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using wrench.auto.repair.cadastro.application.AutoMapper;
using wrench.auto.repair.cadastro.application.Commands;
using wrench.auto.repair.cadastro.application.Commands.Dtos;
using wrench.auto.repair.cadastro.application.tests.Fixture;
using wrench.auto.repair.cadastro.domain.Data;
using wrench.auto.repair.cadastro.domain.Entities;
using wrench.auto.repair.cadastro.domain.ValueObjects;

namespace wrench.auto.repair.cadastro.application.tests.ClienteCommand
{
    [Collection(nameof(ClienteCollection))]
    public class ClienteCommandHandlerTests(ClienteFixture _fixture)
    {
        [Fact(DisplayName = "Cadastrar Novo Cliente Com Sucesso")]
        [Trait("Cadastro", "Application")]
        public async Task Cliente_CadastrarNovoCliente_DeveCadastrarComSucesso()
        {
            var enderecoDto = _fixture.GerarEnderecoDtoValido();
            var documento = _fixture.GerarDocumentoValido();
            var telefone = _fixture.GerarNumeroTelefoneValido();
            var email = _fixture.GerarEnderecoEmailValido();
            var nome = _fixture.GerarNomeQualquerValido();

            var loggerFactory = LoggerFactory.Create(builder => { });
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<EnderecoProfile>();
            }, loggerFactory);

            var mapper = config.CreateMapper();
            var endereco = mapper.Map<Endereco>(enderecoDto);

            var cadastrarClienteCommand =
                new CadastrarClienteCommand(documento, nome, telefone, email, enderecoDto);
            var automocker = new AutoMocker();
            var clienteCommandHandler = automocker.CreateInstance<ClienteCommandHandler>();

            automocker.GetMock<IMapper>()
             .Setup(c => c.Map<Endereco>(It.IsAny<EnderecoDto>()))
             .Returns(endereco);

            automocker.GetMock<IClienteRepository>()
                .Setup(c => c.ObterPorDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Cliente?>(null));

            automocker.GetMock<IClienteRepository>()
                 .Setup(c => c.UnitOfWork.CommitAsync())
                 .Returns(Task.FromResult<bool>(true));

            var result = await clienteCommandHandler.Handle(cadastrarClienteCommand, CancellationToken.None);

            Assert.True(result.Sucesso);

            automocker.GetMock<IClienteRepository>()
                .Verify(c => c.ObterPorDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IClienteRepository>()
                .Verify(c => c.Adicionar(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IClienteRepository>()
                .Verify(c => c.UnitOfWork.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "Cadastrar Cliente Documento Existente Não Deve Cadastrar")]
        [Trait("Cadastro", "Application")]
        public async Task CadastrarCliente_DocumentoExistente_NaoDeveCadastrar()
        {
            var enderecoDto = _fixture.GerarEnderecoDtoValido();
            var documento = _fixture.GerarDocumentoValido();
            var telefone = _fixture.GerarNumeroTelefoneValido();
            var email = _fixture.GerarEnderecoEmailValido();
            var nome = _fixture.GerarNomeQualquerValido();
            var clienteExistente = _fixture.GerarClienteValido();
            var mapper = _fixture.ConfigurarMapeamentoEGerarMapper();
            var endereco = mapper.Map<Endereco>(enderecoDto);

            var cadastrarClienteCommand =
                new CadastrarClienteCommand(documento, nome, telefone, email, enderecoDto);
            var automocker = new AutoMocker();
            var clienteCommandHandler = automocker.CreateInstance<ClienteCommandHandler>();

            automocker.GetMock<IMapper>()
             .Setup(c => c.Map<Endereco>(It.IsAny<EnderecoDto>()))
             .Returns(endereco);

            automocker.GetMock<IClienteRepository>()
                .Setup(c => c.ObterPorDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Cliente?>(clienteExistente));

            var result = await clienteCommandHandler.Handle(cadastrarClienteCommand, CancellationToken.None);

            Assert.False(result.Sucesso);

            automocker.GetMock<IClienteRepository>()
                .Verify(c => c.ObterPorDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IClienteRepository>()
                .Verify(c => c.Adicionar(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);

            automocker.GetMock<IClienteRepository>()
                .Verify(c => c.UnitOfWork.CommitAsync(), Times.Never);
        }

        [Fact(DisplayName = "Atualizar Cliente Com Sucesso")]
        [Trait("Cadastro", "Application")]
        public async Task Cliente_AtualizarCliente_DeveAtualizarComSucesso()
        {
            var clienteId = Guid.NewGuid();
            var enderecoDto = _fixture.GerarEnderecoDtoValido();
            var documento = _fixture.GerarDocumentoValido();
            var telefone = _fixture.GerarNumeroTelefoneValido();
            var email = _fixture.GerarEnderecoEmailValido();
            var nome = _fixture.GerarNomeQualquerValido();
            var clienteLocalizado = _fixture.GerarClienteValido();

            var loggerFactory = LoggerFactory.Create(builder => { });
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<EnderecoProfile>();
            }, loggerFactory);

            var mapper = config.CreateMapper();
            var endereco = mapper.Map<Endereco>(enderecoDto);

            var atualizarClienteCommand =
                new AtualizarClienteCommand(clienteId, nome, telefone, email, enderecoDto);
            var automocker = new AutoMocker();
            var clienteCommandHandler = automocker.CreateInstance<ClienteCommandHandler>();

            automocker.GetMock<IMapper>()
             .Setup(c => c.Map<Endereco>(It.IsAny<EnderecoDto>()))
             .Returns(endereco);

            automocker.GetMock<IClienteRepository>()
                .Setup(c => c.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Cliente?>(clienteLocalizado));

            automocker.GetMock<IClienteRepository>()
                 .Setup(c => c.UnitOfWork.CommitAsync())
                 .Returns(Task.FromResult<bool>(true));

            var result = await clienteCommandHandler
                .Handle(atualizarClienteCommand, CancellationToken.None);

            Assert.True(result.Sucesso);

            automocker.GetMock<IClienteRepository>()
                .Verify(c => c.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IClienteRepository>()
                .Verify(c => c.Atualizar(It.IsAny<Cliente>()), Times.Once);

            automocker.GetMock<IClienteRepository>()
                .Verify(c => c.UnitOfWork.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "Atualizar Cliente Inexistente")]
        [Trait("Cadastro", "Application")]
        public async Task AtualizarCliente_ClienteInexistente_DeveRetornarErro()
        {
            var clienteId = Guid.NewGuid();
            var enderecoDto = _fixture.GerarEnderecoDtoValido();
            var documento = _fixture.GerarDocumentoValido();
            var telefone = _fixture.GerarNumeroTelefoneValido();
            var email = _fixture.GerarEnderecoEmailValido();
            var nome = _fixture.GerarNomeQualquerValido();
            var clienteLocalizado = _fixture.GerarClienteValido();

            var loggerFactory = LoggerFactory.Create(builder => { });
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<EnderecoProfile>();
            }, loggerFactory);

            var mapper = config.CreateMapper();
            var endereco = mapper.Map<Endereco>(enderecoDto);

            var atualizarClienteCommand =
                new AtualizarClienteCommand(clienteId, nome, telefone, email, enderecoDto);
            var automocker = new AutoMocker();
            var clienteCommandHandler = automocker.CreateInstance<ClienteCommandHandler>();

            automocker.GetMock<IMapper>()
             .Setup(c => c.Map<Endereco>(It.IsAny<EnderecoDto>()))
             .Returns(endereco);

            automocker.GetMock<IClienteRepository>()
                .Setup(c => c.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Cliente?>(null));

            automocker.GetMock<IClienteRepository>()
                 .Setup(c => c.UnitOfWork.CommitAsync())
                 .Returns(Task.FromResult<bool>(true));

            var result = await clienteCommandHandler
                .Handle(atualizarClienteCommand, CancellationToken.None);

            Assert.False(result.Sucesso);

            automocker.GetMock<IClienteRepository>()
                .Verify(c => c.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IClienteRepository>()
                .Verify(c => c.Atualizar(It.IsAny<Cliente>()), Times.Never);

            automocker.GetMock<IClienteRepository>()
                .Verify(c => c.UnitOfWork.CommitAsync(), Times.Never);
        }
    }
}
