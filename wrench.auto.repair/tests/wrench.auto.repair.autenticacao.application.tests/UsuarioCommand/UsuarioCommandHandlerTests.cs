using Moq;
using Moq.AutoMock;
using wrench.auto.repair.autenticacao.application.Commands;
using wrench.auto.repair.autenticacao.application.tests.Fixtures;
using wrench.auto.repair.autenticacao.domain.Data;
using wrench.auto.repair.autenticacao.domain.Entities;
using wrench.auto.repair.autenticacao.domain.Models;
using wrench.auto.repair.autenticacao.domain.Security;
using wrench.auto.repair.autenticacao.infra.Security;
using wrench.auto.repair.core.Errors;
using wrench.auto.repair.core.ValueObjects;

namespace wrench.auto.repair.autenticacao.application.tests.UsuarioCommand
{
    [Collection(nameof(UsuarioCollection))]
    public class UsuarioCommandHandlerTests(UsuarioFixture _fixture)
    {
        [Fact(DisplayName = "Cadastrar Novo Usuário Com Sucesso")]
        [Trait("Autenticacao", "Application")]
        public async Task CriarUsuario_NovoUsuario_DeveCadastrarComSucesso()
        {
            var email = _fixture.GerarEmail();
            var senha = _fixture.GerarSenha(24);
            var hashSenha = _fixture.GerarHashSenha(senha);
            var criarUsuarioCommand = new CriarUsuarioCommand(email, senha, Guid.NewGuid(), true);
            var mocker = new AutoMocker();
            var usuarioHandler = mocker.CreateInstance<UsuarioCommandHandler>();

            mocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.UnitOfWork.CommitAsync())
                .Returns(Task.FromResult(true));

            mocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.Adicionar(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            mocker.GetMock<IPasswordHasher>()
               .Setup(u => u.GerarHash(It.IsAny<string>()))
               .Returns(hashSenha);

            mocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.ObterPerfilPorIdAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult<Perfil?>(_fixture.GerarPerfil()));

            var result = await usuarioHandler.Handle(criarUsuarioCommand, CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.Equal(ResultadoStatusEnum.CRIADO, result.ResultadoStatus);
            mocker.GetMock<IUsuarioRepository>().Verify(
                u => u.ObterPorEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()),
                Times.Once
            );

            mocker.GetMock<IUsuarioRepository>().Verify(
                u => u.ObterPerfilPorIdAsync(It.IsAny<Guid>()),
                Times.Once
            );

            mocker.GetMock<IUsuarioRepository>().Verify(
                u => u.Adicionar(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact(DisplayName = "Cadastrar Usuário E-mail Existente Deve Retornar Erro")]
        [Trait("Autenticacao", "Application")]
        public async Task CriarUsuario_EmailExistente_DeveRetornarErro()
        {
            var usuarioExistente = _fixture.GerarUsuario();
            var criarUsuarioCommand = new CriarUsuarioCommand(usuarioExistente.Email.Endereco, null, Guid.NewGuid(), true);
            var mocker = new AutoMocker();
            var usuarioHandler = mocker.CreateInstance<UsuarioCommandHandler>();

            mocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.ObterPorEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Usuario?>(usuarioExistente));

            var result = await usuarioHandler.Handle(criarUsuarioCommand, CancellationToken.None);

            Assert.False(result.Sucesso);
            Assert.Equal(TipoErroEnum.VALIDACAO, result.TipoErro);
            Assert.Contains("Usuário já cadastrado", result.Erros);

            mocker.GetMock<IUsuarioRepository>().Verify(
                u => u.ObterPorEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()),
                Times.Once
            );

            mocker.GetMock<IUsuarioRepository>().Verify(
                u => u.ObterPerfilPorIdAsync(It.IsAny<Guid>()),
                Times.Never
            );

            mocker.GetMock<IUsuarioRepository>().Verify(
                u => u.Adicionar(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        [Fact(DisplayName = "Autenticar Usuário Com Sucesso")]
        [Trait("Autenticacao", "Application")]
        public async Task AutenticarUsuario_CredenciaisValidas_DeveRetornarTokenDeAcesso()
        {
            var senha = _fixture.GerarSenha(24);
            var usuario = _fixture.GerarUsuario();
            var perfil = _fixture.GerarPerfil();
            var hasher = new PasswordHasher();

            usuario.DefinirSenha(hasher.GerarHash(senha));
            usuario.AlterarPerfil(perfil);
            var autenticarUsuarioCommand = new AutenticarUsuarioCommand(usuario.Email.Endereco, senha);
            var autoMocker = new AutoMocker();
            var usuarioCommandHandler = autoMocker.CreateInstance<UsuarioCommandHandler>();

            autoMocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.ObterPorEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Usuario?>(usuario));

            autoMocker.GetMock<IPasswordHasher>()
                .Setup(u => u.ValidarSenha(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            autoMocker.GetMock<IJwtTokenGenerator>()
                .Setup(j => j.GerarToken(It.IsAny<Usuario>()))
                .Returns(new TokenAcesso(It.IsAny<string>(), usuario.Email.Endereco, usuario.Perfil.Nome));

            var result = await usuarioCommandHandler.Handle(autenticarUsuarioCommand, CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.NotNull(result.Valor);
            Assert.Equal(usuario.Email.Endereco, result.Valor.Username);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.ObterPorEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()), Times.Once);

            autoMocker.GetMock<IPasswordHasher>()
                .Verify(u => u.ValidarSenha(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            autoMocker.GetMock<IJwtTokenGenerator>()
                .Verify(u => u.GerarToken(It.IsAny<Usuario>()), Times.Once);
        }

        [Fact(DisplayName = "Autenticar Usuário Inexistente")]
        [Trait("Autenticacao", "Application")]
        public async Task AutenticarUsuario_UsuarioInexistente_DeveRetornarNaoAutorizado()
        {
            var senha = _fixture.GerarSenha(24);
            var usuario = _fixture.GerarUsuario();
            var perfil = _fixture.GerarPerfil();
            var hasher = new PasswordHasher();

            usuario.DefinirSenha(hasher.GerarHash(senha));
            usuario.AlterarPerfil(perfil);
            var autenticarUsuarioCommand = new AutenticarUsuarioCommand(usuario.Email.Endereco, senha);
            var autoMocker = new AutoMocker();
            var usuarioCommandHandler = autoMocker.CreateInstance<UsuarioCommandHandler>();

            autoMocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.ObterPorEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Usuario?>(null));

            var result = await usuarioCommandHandler.Handle(autenticarUsuarioCommand, CancellationToken.None);

            Assert.False(result.Sucesso);
            Assert.Null(result.Valor);
            Assert.Equal(result.TipoErro, TipoErroEnum.NAO_AUTORIZADO);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.ObterPorEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()), Times.Once);

            autoMocker.GetMock<IPasswordHasher>()
                .Verify(u => u.ValidarSenha(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

            autoMocker.GetMock<IJwtTokenGenerator>()
                .Verify(u => u.GerarToken(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact(DisplayName = "Autenticar Usuário Senha Inválida")]
        [Trait("Autenticacao", "Application")]
        public async Task AutenticarUsuario_SenhaInvalida_DeveRetornarNaoAutorizado()
        {
            var senha = _fixture.GerarSenha(24);
            var usuario = _fixture.GerarUsuario();
            var perfil = _fixture.GerarPerfil();
            var hasher = new PasswordHasher();

            usuario.DefinirSenha(hasher.GerarHash(senha));
            usuario.AlterarPerfil(perfil);
            var autenticarUsuarioCommand = new AutenticarUsuarioCommand(usuario.Email.Endereco, senha);
            var autoMocker = new AutoMocker();
            var usuarioCommandHandler = autoMocker.CreateInstance<UsuarioCommandHandler>();

            autoMocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.ObterPorEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Usuario?>(usuario));

            autoMocker.GetMock<IPasswordHasher>()
            .Setup(u => u.ValidarSenha(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

            var result = await usuarioCommandHandler.Handle(autenticarUsuarioCommand, CancellationToken.None);

            Assert.False(result.Sucesso);
            Assert.Null(result.Valor);
            Assert.Equal(result.TipoErro, TipoErroEnum.NAO_AUTORIZADO);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.ObterPorEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()), Times.Once);

            autoMocker.GetMock<IPasswordHasher>()
                .Verify(u => u.ValidarSenha(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            autoMocker.GetMock<IJwtTokenGenerator>()
                .Verify(u => u.GerarToken(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact(DisplayName = "Ativar Usuário Com Sucesso")]
        [Trait("Autenticacao", "Application")]
        public async Task AtivarUsuario_UsuarioEncontrado_DeveAtivarUsuarioComSucesso()
        {
            var usuario = _fixture.GerarUsuario(ativo: false);

            var ativarUsuarioCommand = new AtivarUsuarioCommand(usuario.Id);
            var autoMocker = new AutoMocker();
            var usuarioCommandHandler = autoMocker.CreateInstance<UsuarioCommandHandler>();

            autoMocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Usuario?>(usuario));

            autoMocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.UnitOfWork.CommitAsync())
                .Returns(Task.FromResult(true));

            var result = await usuarioCommandHandler.Handle(ativarUsuarioCommand, CancellationToken.None);

            Assert.True(result.Sucesso);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.Atualizar(It.IsAny<Usuario>()), Times.Once);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.UnitOfWork.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "Ativar Usuario Não Encontrado")]
        [Trait("Autenticacao", "Application")]
        public async Task AtivarUsuario_UsuarioNaoEncontrado_DeveRetornarErro()
        {
            var ativarUsuarioCommand = new AtivarUsuarioCommand(Guid.NewGuid());
            var autoMocker = new AutoMocker();
            var usuarioCommandHandler = autoMocker.CreateInstance<UsuarioCommandHandler>();

            autoMocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Usuario?>(null));

            autoMocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.UnitOfWork.CommitAsync())
                .Returns(Task.FromResult(true));

            var result = await usuarioCommandHandler.Handle(ativarUsuarioCommand, CancellationToken.None);

            Assert.False(result.Sucesso);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.Atualizar(It.IsAny<Usuario>()), Times.Never);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.UnitOfWork.CommitAsync(), Times.Never);
        }

        [Fact(DisplayName = "Inativar Usuário Com Sucesso")]
        [Trait("Autenticacao", "Application")]
        public async Task InativarUsuario_UsuarioEncontrado_DeveInativarUsuarioComSucesso()
        {
            var perfil = _fixture.GerarPerfil("Cliente");
            var usuario = _fixture.GerarUsuario();
            usuario.AlterarPerfil(perfil);

            var inativarUsuarioCommand = new InativarUsuarioCommand(usuario.Id);
            var autoMocker = new AutoMocker();
            var usuarioCommandHandler = autoMocker.CreateInstance<UsuarioCommandHandler>();

            autoMocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Usuario?>(usuario));

            autoMocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.UnitOfWork.CommitAsync())
                .Returns(Task.FromResult(true));

            var result = await usuarioCommandHandler.Handle(inativarUsuarioCommand, CancellationToken.None);

            Assert.True(result.Sucesso);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.Atualizar(It.IsAny<Usuario>()), Times.Once);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.UnitOfWork.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "Inativar Usuário Não Encontrado")]
        [Trait("Autenticacao", "Application")]
        public async Task InativarUsuario_UsuarioNaoEncontrado_DeveRetornarErro()
        {
            var inativarUsuarioCommand = new InativarUsuarioCommand(Guid.NewGuid());
            var autoMocker = new AutoMocker();
            var usuarioCommandHandler = autoMocker.CreateInstance<UsuarioCommandHandler>();

            autoMocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Usuario?>(null));

            autoMocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.UnitOfWork.CommitAsync())
                .Returns(Task.FromResult(true));

            var result = await usuarioCommandHandler.Handle(inativarUsuarioCommand, CancellationToken.None);

            Assert.False(result.Sucesso);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.Atualizar(It.IsAny<Usuario>()), Times.Never);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.UnitOfWork.CommitAsync(), Times.Never);
        }

        [Fact(DisplayName = "Resetar Senha Usuário Com Sucesso")]
        [Trait("Autenticacao", "Application")]
        public async Task ResetarSenhaUsuario_UsuarioNaoAdminEAtivo_DeveResetarComSucesso()
        {
            var perfil = _fixture.GerarPerfil("Cliente");
            var usuario = _fixture.GerarUsuario();
            usuario.AlterarPerfil(perfil);

            var resetarSenhaUsuarioCommand = new ResetarSenhaUsuarioCommand(usuario.Id);
            var autoMocker = new AutoMocker();
            var usuarioCommandHandler = autoMocker.CreateInstance<UsuarioCommandHandler>();
            var hasher = new PasswordHasher();
            var passwordHash = hasher.GerarHash("My@Secret@123!");

            usuario.DefinirSenha(passwordHash);

            autoMocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Usuario?>(usuario));

            autoMocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.UnitOfWork.CommitAsync())
                .Returns(Task.FromResult(true));

            var result = await usuarioCommandHandler.Handle(resetarSenhaUsuarioCommand, CancellationToken.None);

            Assert.True(result.Sucesso);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.Atualizar(It.IsAny<Usuario>()), Times.Once);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.UnitOfWork.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "Resetar Senha Usuário Admin e Ativo Não Deve Resetar")]
        [Trait("Autenticacao", "Application")]
        public async Task ResetarSenhaUsuario_UsuarioAdminEAtivo_NaoDeveResetar()
        {
            var perfil = _fixture.GerarPerfil("Admin");
            var usuario = _fixture.GerarUsuario();
            usuario.AlterarPerfil(perfil);

            var resetarSenhaUsuarioCommand = new ResetarSenhaUsuarioCommand(usuario.Id);
            var autoMocker = new AutoMocker();
            var usuarioCommandHandler = autoMocker.CreateInstance<UsuarioCommandHandler>();
            var hasher = new PasswordHasher();
            var passwordHash = hasher.GerarHash("My@Secret@123!");

            usuario.DefinirSenha(passwordHash);

            autoMocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Usuario?>(usuario));

            autoMocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.UnitOfWork.CommitAsync())
                .Returns(Task.FromResult(true));

            var result = await usuarioCommandHandler.Handle(resetarSenhaUsuarioCommand, CancellationToken.None);

            Assert.False(result.Sucesso);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.Atualizar(It.IsAny<Usuario>()), Times.Never);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.UnitOfWork.CommitAsync(), Times.Never);
        }

        [Fact(DisplayName = "Resetar Senha Usuário Inativo Não Deve Resetar")]
        [Trait("Autenticacao", "Application")]
        public async Task ResetarSenhaUsuario_UsuarioInativo_NaoDeveResetar()
        {
            var perfil = _fixture.GerarPerfil("Cliente");
            var usuario = _fixture.GerarUsuario(ativo: false);
            usuario.AlterarPerfil(perfil);

            var resetarSenhaUsuarioCommand = new ResetarSenhaUsuarioCommand(usuario.Id);
            var autoMocker = new AutoMocker();
            var usuarioCommandHandler = autoMocker.CreateInstance<UsuarioCommandHandler>();
            var hasher = new PasswordHasher();
            var passwordHash = hasher.GerarHash("My@Secret@123!");

            usuario.DefinirSenha(passwordHash);

            autoMocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Usuario?>(usuario));

            autoMocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.UnitOfWork.CommitAsync())
                .Returns(Task.FromResult(true));

            var result = await usuarioCommandHandler.Handle(resetarSenhaUsuarioCommand, CancellationToken.None);

            Assert.False(result.Sucesso);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.Atualizar(It.IsAny<Usuario>()), Times.Never);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.UnitOfWork.CommitAsync(), Times.Never);
        }

        [Fact(DisplayName = "Resetar Senha Usuário Com Senha Já Resetada Deve Retornar Sucesso")]
        [Trait("Autenticacao", "Application")]
        public async Task ResetarSenhaUsuario_UsuarioNaoAdminEAtivoComSenhaJaResetada_DeveRetornarSucesso()
        {
            var perfil = _fixture.GerarPerfil("Cliente");
            var usuario = _fixture.GerarUsuario();
            usuario.AlterarPerfil(perfil);

            var resetarSenhaUsuarioCommand = new ResetarSenhaUsuarioCommand(usuario.Id);
            var autoMocker = new AutoMocker();
            var usuarioCommandHandler = autoMocker.CreateInstance<UsuarioCommandHandler>();

            autoMocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Usuario?>(usuario));

            autoMocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.UnitOfWork.CommitAsync())
                .Returns(Task.FromResult(true));

            var result = await usuarioCommandHandler.Handle(resetarSenhaUsuarioCommand, CancellationToken.None);

            Assert.True(result.Sucesso);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.Atualizar(It.IsAny<Usuario>()), Times.Never);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.UnitOfWork.CommitAsync(), Times.Never);
        }

        [Fact(DisplayName = "Primeiro Acesso Usuário Deve Cadastrar Senha")]
        [Trait("Autenticacao", "Application")]
        public async Task Usuario_PrimeiroAcessoUsuario_DeveCadastrarSenha()
        {
            var passwordHash = new PasswordHasher();
            var perfil = _fixture.GerarPerfil("Cliente");
            var usuario = _fixture.GerarUsuario();
            var senhaSemHash = _fixture.GerarSenha(24);
            var hashSenha = passwordHash.GerarHash(senhaSemHash);
            usuario.AlterarPerfil(perfil);

            var primeiroAcessoUsuarioCommand = new PrimeiroAcessoUsuarioCommand(usuario.Email.Endereco, senhaSemHash);
            var autoMocker = new AutoMocker();
            var usuarioCommandHandler = autoMocker.CreateInstance<UsuarioCommandHandler>();

            autoMocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.ObterPorEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Usuario?>(usuario));

            autoMocker.GetMock<IPasswordHasher>()
                .Setup(u => u.GerarHash(senhaSemHash))
                .Returns(hashSenha);

            autoMocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.UnitOfWork.CommitAsync())
                .Returns(Task.FromResult(true));

            var result = await usuarioCommandHandler.Handle(primeiroAcessoUsuarioCommand, CancellationToken.None);

            Assert.True(result.Sucesso);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.ObterPorEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()), Times.Once);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.Atualizar(It.IsAny<Usuario>()), Times.Once);

            autoMocker.GetMock<IPasswordHasher>()
               .Verify(u => u.GerarHash(It.IsAny<string>()), Times.Once);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.UnitOfWork.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "Primeiro Acesso Usuário Nao Encontrado")]
        [Trait("Autenticacao", "Application")]
        public async Task PrimeiroAcessoUsuario_UsuarioNaoEncontrado_DeveRetornarErro()
        {
            var passwordHash = new PasswordHasher();
            var email = _fixture.GerarEmail();
            var senhaSemHash = _fixture.GerarSenha(24);
            var hashSenha = passwordHash.GerarHash(senhaSemHash);

            var primeiroAcessoUsuarioCommand = new PrimeiroAcessoUsuarioCommand(email, senhaSemHash);
            var autoMocker = new AutoMocker();
            var usuarioCommandHandler = autoMocker.CreateInstance<UsuarioCommandHandler>();

            autoMocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.ObterPorEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Usuario?>(null));

            autoMocker.GetMock<IPasswordHasher>()
                .Setup(u => u.GerarHash(senhaSemHash))
                .Returns(hashSenha);

            autoMocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.UnitOfWork.CommitAsync())
                .Returns(Task.FromResult(true));

            var result = await usuarioCommandHandler.Handle(primeiroAcessoUsuarioCommand, CancellationToken.None);

            Assert.False(result.Sucesso);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.ObterPorEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()), Times.Once);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.Atualizar(It.IsAny<Usuario>()), Times.Never);

            autoMocker.GetMock<IPasswordHasher>()
               .Verify(u => u.GerarHash(It.IsAny<string>()), Times.Never);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.UnitOfWork.CommitAsync(), Times.Never);
        }

        [Fact(DisplayName = "Primeiro Acesso Usuário Admin")]
        [Trait("Autenticacao", "Application")]
        public async Task PrimeiroAcessoUsuario_UsuarioAdmin_NaoDeveResetarSenha()
        {
            var passwordHash = new PasswordHasher();
            var perfil = _fixture.GerarPerfil("Admin");
            var usuario = _fixture.GerarUsuario();
            var senhaSemHash = _fixture.GerarSenha(24);
            var hashSenha = passwordHash.GerarHash(senhaSemHash);
            usuario.AlterarPerfil(perfil);

            var primeiroAcessoUsuarioCommand = new PrimeiroAcessoUsuarioCommand(usuario.Email.Endereco, senhaSemHash);
            var autoMocker = new AutoMocker();
            var usuarioCommandHandler = autoMocker.CreateInstance<UsuarioCommandHandler>();

            autoMocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.ObterPorEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Usuario?>(usuario));

            autoMocker.GetMock<IPasswordHasher>()
                .Setup(u => u.GerarHash(senhaSemHash))
                .Returns(hashSenha);

            autoMocker.GetMock<IUsuarioRepository>()
                .Setup(u => u.UnitOfWork.CommitAsync())
                .Returns(Task.FromResult(true));

            var result = await usuarioCommandHandler.Handle(primeiroAcessoUsuarioCommand, CancellationToken.None);

            Assert.False(result.Sucesso);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.ObterPorEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()), Times.Once);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.Atualizar(It.IsAny<Usuario>()), Times.Never);

            autoMocker.GetMock<IPasswordHasher>()
               .Verify(u => u.GerarHash(It.IsAny<string>()), Times.Never);

            autoMocker.GetMock<IUsuarioRepository>()
                .Verify(u => u.UnitOfWork.CommitAsync(), Times.Never);
        }
    }
}
