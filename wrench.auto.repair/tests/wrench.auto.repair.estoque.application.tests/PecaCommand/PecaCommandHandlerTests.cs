using System.Linq.Expressions;
using Moq;
using Moq.AutoMock;
using AutoMapper;
using wrench.auto.repair.core.Messages.CommonMessages.IntegratedQueries;
using wrench.auto.repair.core.Messages.CommonMessages.IntegratedQueries.Dtos;
using wrench.auto.repair.estoque.application.Commands;
using wrench.auto.repair.estoque.application.tests.Fixtures;
using wrench.auto.repair.estoque.domain.Data;
using wrench.auto.repair.estoque.domain.Entities;

namespace wrench.auto.repair.estoque.application.tests.PecaCommand
{
    public class PecaCommandHandlerTests
    {
        [Fact(DisplayName = "Ativar Peça Existente Com Sucesso")]
        [Trait("Estoque", "Application")]
        public async Task AtivarPeca_PecaExistente_DeveAtivarPeca()
        {
            var pecaInativa = new Peca("Pneu", "Aro 15", 100, 10, false, DateTime.UtcNow);
            var ativarPecaCommand =
                new AtivarPecaCommand(Guid.NewGuid());
            var automocker = new AutoMocker();

            var pecaCommandHandler = automocker.CreateInstance<PecaCommandHandler>();

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Peca?>(pecaInativa));

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.UnitOfWork.CommitAsync())
                .Returns(Task.FromResult(true));

            var resultado = await pecaCommandHandler
                .Handle(ativarPecaCommand, CancellationToken.None);

            Assert.True(resultado.Sucesso);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.Atualizar(It.IsAny<Peca>()), Times.Once);

            automocker.GetMock<IPecaRepository>()
               .Verify(p => p.UnitOfWork.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "Ativar Peça Inexistente Deve Retornar Erro")]
        [Trait("Estoque", "Application")]
        public async Task AtivarPeca_PecaInexistente_DeveRetornarErro()
        {
            var pecaInativa = new Peca("Pneu", "Aro 15", 100, 10, false, DateTime.UtcNow);
            var ativarPecaCommand =
                new AtivarPecaCommand(Guid.NewGuid());
            var automocker = new AutoMocker();

            var pecaCommandHandler = automocker.CreateInstance<PecaCommandHandler>();

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Peca?>(null));

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.UnitOfWork.CommitAsync())
                .Returns(Task.FromResult(true));

            var resultado = await pecaCommandHandler
                .Handle(ativarPecaCommand, CancellationToken.None);

            Assert.False(resultado.Sucesso);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.Atualizar(It.IsAny<Peca>()), Times.Never);

            automocker.GetMock<IPecaRepository>()
               .Verify(p => p.UnitOfWork.CommitAsync(), Times.Never);
        }

        [Fact(DisplayName = "Inativar Peça Existente Com Sucesso")]
        [Trait("Estoque", "Application")]
        public async Task InativarPeca_PecaExistente_DeveAtivarPeca()
        {
            var pecaAtiva = new Peca("Pneu", "Aro 15", 100, 10, true, DateTime.UtcNow);
            var inativarPecaCommand =
                new InativarPecaCommand(Guid.NewGuid());
            var automocker = new AutoMocker();

            var pecaCommandHandler = automocker.CreateInstance<PecaCommandHandler>();

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Peca?>(pecaAtiva));

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.UnitOfWork.CommitAsync())
                .Returns(Task.FromResult(true));

            var resultado = await pecaCommandHandler
                .Handle(inativarPecaCommand, CancellationToken.None);

            Assert.True(resultado.Sucesso);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.Atualizar(It.IsAny<Peca>()), Times.Once);

            automocker.GetMock<IPecaRepository>()
               .Verify(p => p.UnitOfWork.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "Inativar Peça Inexistente Deve Retornar Erro")]
        [Trait("Estoque", "Application")]
        public async Task InativarPeca_PecaInexistente_DeveRetornarErro()
        {
            var pecaInativa = new Peca("Pneu", "Aro 15", 100, 10, true, DateTime.UtcNow);
            var inativarPecaCommand =
                new InativarPecaCommand(Guid.NewGuid());
            var automocker = new AutoMocker();

            var pecaCommandHandler = automocker.CreateInstance<PecaCommandHandler>();

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Peca?>(null));

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.UnitOfWork.CommitAsync())
                .Returns(Task.FromResult(true));

            var resultado = await pecaCommandHandler
                .Handle(inativarPecaCommand, CancellationToken.None);

            Assert.False(resultado.Sucesso);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.Atualizar(It.IsAny<Peca>()), Times.Never);

            automocker.GetMock<IPecaRepository>()
               .Verify(p => p.UnitOfWork.CommitAsync(), Times.Never);
        }

        [Fact(DisplayName = "Repor Peça Existente Com Sucesso")]
        [Trait("Estoque", "Application")]
        public async Task ReporPeca_PecaExistente_DeveReporPeca()
        {
            var pecaAtiva = new Peca("Pneu", "Aro 15", 100, 10, true, DateTime.UtcNow);
            var reporPecaCommand =
                new ReporPecaCommand(Guid.NewGuid(), 10);
            var automocker = new AutoMocker();

            var pecaCommandHandler = automocker.CreateInstance<PecaCommandHandler>();

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Peca?>(pecaAtiva));

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.UnitOfWork.CommitAsync())
                .Returns(Task.FromResult(true));

            var resultado = await pecaCommandHandler
                .Handle(reporPecaCommand, CancellationToken.None);

            Assert.True(resultado.Sucesso);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.Atualizar(It.IsAny<Peca>()), Times.Once);

            automocker.GetMock<IPecaRepository>()
               .Verify(p => p.UnitOfWork.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "Repor Peça Inexistente Com Sucesso")]
        [Trait("Estoque", "Application")]
        public async Task ReporPeca_PecaInexistente_DeveRetornarErro()
        {
            var pecaAtiva = new Peca("Pneu", "Aro 15", 100, 10, true, DateTime.UtcNow);
            var reporPecaCommand =
                new ReporPecaCommand(Guid.NewGuid(), 10);
            var automocker = new AutoMocker();

            var pecaCommandHandler = automocker.CreateInstance<PecaCommandHandler>();

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Peca?>(null));

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.UnitOfWork.CommitAsync())
                .Returns(Task.FromResult(true));

            var resultado = await pecaCommandHandler
                .Handle(reporPecaCommand, CancellationToken.None);

            Assert.False(resultado.Sucesso);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.Atualizar(It.IsAny<Peca>()), Times.Never);

            automocker.GetMock<IPecaRepository>()
               .Verify(p => p.UnitOfWork.CommitAsync(), Times.Never);
        }

        [Fact(DisplayName = "Baixar Estoque Peça Existente Com Sucesso")]
        [Trait("Estoque", "Application")]
        public async Task BaixarEstoque_PecaExistente_DeveBaixarPecaNoEstoque()
        {
            var pecaAtiva = new Peca("Pneu", "Aro 15", 100, 10, true, DateTime.UtcNow);
            var baixarPecaCommand =
                new BaixarPecaCommand(Guid.NewGuid(), 10);
            var automocker = new AutoMocker();

            var pecaCommandHandler = automocker.CreateInstance<PecaCommandHandler>();

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Peca?>(pecaAtiva));

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.UnitOfWork.CommitAsync())
                .Returns(Task.FromResult(true));

            var resultado = await pecaCommandHandler
                .Handle(baixarPecaCommand, CancellationToken.None);

            Assert.True(resultado.Sucesso);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.Atualizar(It.IsAny<Peca>()), Times.Once);

            automocker.GetMock<IPecaRepository>()
               .Verify(p => p.UnitOfWork.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "Baixar Estoque Peça Inexistente")]
        [Trait("Estoque", "Application")]
        public async Task BaixarEstoque_PecaInexistente_DeveRetornarErro()
        {
            var pecaAtiva = new Peca("Pneu", "Aro 15", 100, 10, true, DateTime.UtcNow);
            var baixarPecaCommand =
                new BaixarPecaCommand(Guid.NewGuid(), 10);
            var automocker = new AutoMocker();

            var pecaCommandHandler = automocker.CreateInstance<PecaCommandHandler>();

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Peca?>(null));

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.UnitOfWork.CommitAsync())
                .Returns(Task.FromResult(true));

            var resultado = await pecaCommandHandler
                .Handle(baixarPecaCommand, CancellationToken.None);

            Assert.False(resultado.Sucesso);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.Atualizar(It.IsAny<Peca>()), Times.Never);

            automocker.GetMock<IPecaRepository>()
               .Verify(p => p.UnitOfWork.CommitAsync(), Times.Never);
        }

        [Fact(DisplayName = "Atualizar Peça Existente Com Sucesso")]
        [Trait("Estoque", "Application")]
        public async Task AtualizarPeca_PecaExistente_DeveAtualizarComSucesso()
        {
            var peca = new Peca("Pneu", "Aro 15", 100, 10, true, DateTime.UtcNow);
            var atualizarPecaCommand =
                new AtualizarPecaCommand(Guid.NewGuid(), "Pneu XPTO", "Aro 15 sem camara", 200, true);
            var automocker = new AutoMocker();

            var pecaCommandHandler = automocker.CreateInstance<PecaCommandHandler>();

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Peca?>(peca));

            automocker.GetMock<IPecaRepository>()
             .Setup(p => p.ObterPorNomeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .Returns(Task.FromResult<Peca?>(null));

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.UnitOfWork.CommitAsync())
                .Returns(Task.FromResult(true));

            var resultado = await pecaCommandHandler
                .Handle(atualizarPecaCommand, CancellationToken.None);

            Assert.True(resultado.Sucesso);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.ObterPorNomeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.Atualizar(It.IsAny<Peca>()), Times.Once);

            automocker.GetMock<IPecaRepository>()
               .Verify(p => p.UnitOfWork.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "Atualizar Peça Inexistente Com Sucesso")]
        [Trait("Estoque", "Application")]
        public async Task AtualizarPeca_PecaInexistente_DeveAtualizarComSucesso()
        {
            var peca = new Peca("Pneu", "Aro 15", 100, 10, true, DateTime.UtcNow);
            var atualizarPecaCommand =
                new AtualizarPecaCommand(Guid.NewGuid(), "Pneu XPTO", "Aro 15 sem camara", 200, true);
            var automocker = new AutoMocker();

            var pecaCommandHandler = automocker.CreateInstance<PecaCommandHandler>();

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Peca?>(null));

            automocker.GetMock<IPecaRepository>()
             .Setup(p => p.ObterPorNomeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .Returns(Task.FromResult<Peca?>(null));

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.UnitOfWork.CommitAsync())
                .Returns(Task.FromResult(true));

            var resultado = await pecaCommandHandler
                .Handle(atualizarPecaCommand, CancellationToken.None);

            Assert.False(resultado.Sucesso);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.ObterPorNomeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.Atualizar(It.IsAny<Peca>()), Times.Never);

            automocker.GetMock<IPecaRepository>()
               .Verify(p => p.UnitOfWork.CommitAsync(), Times.Never);
        }

        [Fact(DisplayName = "Atualizar Peça Nome Já Existe Para Outra Peça")]
        [Trait("Estoque", "Application")]
        public async Task AtualizarPeca_NomePecaEmUso_NaoDeveAtualizar()
        {
            var pecaComOutroId = new Peca("Pneu XPTO", "Aro 15", 100, 10, true, DateTime.UtcNow);
            var peca = new Peca("Pneu", "Aro 15", 100, 10, true, DateTime.UtcNow);
            var atualizarPecaCommand =
                new AtualizarPecaCommand(Guid.NewGuid(), "Pneu XPTO", "Aro 15 sem camara", 200, true);
            var automocker = new AutoMocker();

            var pecaCommandHandler = automocker.CreateInstance<PecaCommandHandler>();

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Peca?>(peca));

            automocker.GetMock<IPecaRepository>()
             .Setup(p => p.ObterPorNomeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .Returns(Task.FromResult<Peca?>(pecaComOutroId));

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.UnitOfWork.CommitAsync())
                .Returns(Task.FromResult(true));

            var resultado = await pecaCommandHandler
                .Handle(atualizarPecaCommand, CancellationToken.None);

            Assert.False(resultado.Sucesso);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.ObterPorNomeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.Atualizar(It.IsAny<Peca>()), Times.Never);

            automocker.GetMock<IPecaRepository>()
               .Verify(p => p.UnitOfWork.CommitAsync(), Times.Never);
        }

        [Fact(DisplayName = "Cadastrar Peça Com Sucesso")]
        [Trait("Estoque", "Application")]
        public async Task CadastrarPeca_NovaPeca_DeveCadastrarComSucesso()
        {
            var cadastrarPecaCommand =
                new CadastrarPecaCommand("Pneu XPTO", "Aro 15 sem camara", 200, 10, true);
            var automocker = new AutoMocker();

            var pecaCommandHandler = automocker.CreateInstance<PecaCommandHandler>();

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.ObterPorNomeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Peca?>(null));

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.UnitOfWork.CommitAsync())
                .Returns(Task.FromResult(true));

            var resultado = await pecaCommandHandler
                .Handle(cadastrarPecaCommand, CancellationToken.None);

            Assert.True(resultado.Sucesso);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.ObterPorNomeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.Adicionar(It.IsAny<Peca>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IPecaRepository>()
               .Verify(p => p.UnitOfWork.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "Cadastrar Peça Que Já Existe")]
        [Trait("Estoque", "Application")]
        public async Task CadastrarPeca_PecaExistente_NaoDeveCadastrar()
        {
            var peca = new Peca("Pneu XPTO", "Aro 15", 200, 10, true, DateTime.UtcNow);
            var cadastrarPecaCommand =
                new CadastrarPecaCommand("Pneu XPTO", "Aro 15", 200, 10, true);
            var automocker = new AutoMocker();

            var pecaCommandHandler = automocker.CreateInstance<PecaCommandHandler>();

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.ObterPorNomeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Peca?>(peca));

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.UnitOfWork.CommitAsync())
                .Returns(Task.FromResult(true));

            var resultado = await pecaCommandHandler
                .Handle(cadastrarPecaCommand, CancellationToken.None);

            Assert.False(resultado.Sucesso);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.ObterPorNomeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.Adicionar(It.IsAny<Peca>(), It.IsAny<CancellationToken>()), Times.Never);

            automocker.GetMock<IPecaRepository>()
               .Verify(p => p.UnitOfWork.CommitAsync(), Times.Never);
        }

        [Fact(DisplayName = "Ativar peça já ativa deve retornar sucesso sem persistir")]
        [Trait("Estoque", "Application")]
        public async Task AtivarPeca_JaAtiva_DeveRetornarNoContentSemCommit()
        {
            var pecaAtiva = new Peca("Pneu", "Aro 15", 100, 10, true, DateTime.UtcNow);
            var cmd = new AtivarPecaCommand(Guid.NewGuid());
            var automocker = new AutoMocker();
            var handler = automocker.CreateInstance<PecaCommandHandler>();

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(pecaAtiva);

            var resultado = await handler.Handle(cmd, CancellationToken.None);

            Assert.True(resultado.Sucesso);
            automocker.GetMock<IPecaRepository>().Verify(p => p.Atualizar(It.IsAny<Peca>()), Times.Never);
            automocker.GetMock<IPecaRepository>().Verify(p => p.UnitOfWork.CommitAsync(), Times.Never);
        }

        [Fact(DisplayName = "Inativar peça já inativa deve retornar sucesso sem persistir")]
        [Trait("Estoque", "Application")]
        public async Task InativarPeca_JaInativa_DeveRetornarNoContentSemCommit()
        {
            var pecaInativa = new Peca("Pneu", "Aro 15", 100, 10, false, DateTime.UtcNow);
            var cmd = new InativarPecaCommand(Guid.NewGuid());
            var automocker = new AutoMocker();
            var handler = automocker.CreateInstance<PecaCommandHandler>();

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(pecaInativa);

            var resultado = await handler.Handle(cmd, CancellationToken.None);

            Assert.True(resultado.Sucesso);
            automocker.GetMock<IPecaRepository>().Verify(p => p.Atualizar(It.IsAny<Peca>()), Times.Never);
            automocker.GetMock<IPecaRepository>().Verify(p => p.UnitOfWork.CommitAsync(), Times.Never);
        }

        [Fact(DisplayName = "Ativar peça com identificador vazio deve falhar na validação")]
        [Trait("Estoque", "Application")]
        public async Task AtivarPeca_IdVazio_DeveRetornarValidacao()
        {
            var cmd = new AtivarPecaCommand(Guid.Empty);
            var automocker = new AutoMocker();
            var handler = automocker.CreateInstance<PecaCommandHandler>();

            var resultado = await handler.Handle(cmd, CancellationToken.None);

            Assert.False(resultado.Sucesso);
            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Cadastrar peça deve falhar quando commit não persistir")]
        [Trait("Estoque", "Application")]
        public async Task CadastrarPeca_CommitFalso_DeveRetornarErro()
        {
            var cmd = new CadastrarPecaCommand("Peça Nova", "Descrição", 50, 5, true);
            var automocker = new AutoMocker();
            var handler = automocker.CreateInstance<PecaCommandHandler>();

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.ObterPorNomeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Peca?)null);
            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.UnitOfWork.CommitAsync())
                .ReturnsAsync(false);

            var resultado = await handler.Handle(cmd, CancellationToken.None);

            Assert.False(resultado.Sucesso);
        }

        [Fact(DisplayName = "Repor peça deve falhar quando commit não persistir")]
        [Trait("Estoque", "Application")]
        public async Task ReporPeca_CommitFalso_DeveRetornarErro()
        {
            var peca = new Peca("Pneu", "Aro 15", 100, 10, true, DateTime.UtcNow);
            var cmd = new ReporPecaCommand(Guid.NewGuid(), 2);
            var automocker = new AutoMocker();
            var handler = automocker.CreateInstance<PecaCommandHandler>();

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(peca);
            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.UnitOfWork.CommitAsync())
                .ReturnsAsync(false);

            var resultado = await handler.Handle(cmd, CancellationToken.None);

            Assert.False(resultado.Sucesso);
        }

        [Fact(DisplayName = "Baixar peça deve falhar quando commit não persistir")]
        [Trait("Estoque", "Application")]
        public async Task BaixarPeca_CommitFalso_DeveRetornarErro()
        {
            var peca = new Peca("Pneu", "Aro 15", 100, 10, true, DateTime.UtcNow);
            var cmd = new BaixarPecaCommand(Guid.NewGuid(), 2);
            var automocker = new AutoMocker();
            var handler = automocker.CreateInstance<PecaCommandHandler>();

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(peca);
            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.UnitOfWork.CommitAsync())
                .ReturnsAsync(false);

            var resultado = await handler.Handle(cmd, CancellationToken.None);

            Assert.False(resultado.Sucesso);
        }

        [Fact(DisplayName = "Baixar peça com quantidade inválida deve falhar na validação")]
        [Trait("Estoque", "Application")]
        public async Task BaixarPeca_QuantidadeInvalida_DeveRetornarValidacao()
        {
            var cmd = new BaixarPecaCommand(Guid.NewGuid(), 0);
            var automocker = new AutoMocker();
            var handler = automocker.CreateInstance<PecaCommandHandler>();

            var resultado = await handler.Handle(cmd, CancellationToken.None);

            Assert.False(resultado.Sucesso);
            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Atualizar peça deve falhar quando commit não persistir")]
        [Trait("Estoque", "Application")]
        public async Task AtualizarPeca_CommitFalso_DeveRetornarErro()
        {
            var peca = new Peca("Pneu", "Aro 15", 100, 10, true, DateTime.UtcNow);
            var cmd = new AtualizarPecaCommand(Guid.NewGuid(), "Outro nome", "Nova descrição", 200, false);
            var automocker = new AutoMocker();
            var handler = automocker.CreateInstance<PecaCommandHandler>();

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(peca);
            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.ObterPorNomeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Peca?)null);
            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.UnitOfWork.CommitAsync())
                .ReturnsAsync(false);

            var resultado = await handler.Handle(cmd, CancellationToken.None);

            Assert.False(resultado.Sucesso);
        }

        [Fact(DisplayName = "Atualizar peça com mesmo nome (ignorando maiúsculas) não deve consultar duplicidade por nome")]
        [Trait("Estoque", "Application")]
        public async Task AtualizarPeca_MesmoNomeCaseInsensitive_NaoDeveChamarObterPorNome()
        {
            var peca = new Peca("Pneu Original", "Aro 15", 100, 10, true, DateTime.UtcNow);
            var cmd = new AtualizarPecaCommand(Guid.NewGuid(), "pneu original", "Nova descrição longa", 150, true);
            var automocker = new AutoMocker();
            var handler = automocker.CreateInstance<PecaCommandHandler>();

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(peca);
            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.UnitOfWork.CommitAsync())
                .ReturnsAsync(true);

            var resultado = await handler.Handle(cmd, CancellationToken.None);

            Assert.True(resultado.Sucesso);
            automocker.GetMock<IPecaRepository>()
                .Verify(p => p.ObterPorNomeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "PecaExisteQuery deve executar quando peça existir")]
        [Trait("Estoque", "Application")]
        public async Task PecaExisteQuery_QuandoExiste_DeveRetornarSucesso()
        {
            var peca = new Peca("Pneu", "Aro 15", 100, 10, true, DateTime.UtcNow);
            var query = new PecaExisteQuery(peca.Id);
            var automocker = new AutoMocker();
            var handler = automocker.CreateInstance<PecaCommandHandler>();

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.ObterPorIdAsync(peca.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(peca);

            var resultado = await handler.Handle(query, CancellationToken.None);

            Assert.True(resultado.Sucesso);
        }

        [Fact(DisplayName = "PecaExisteQuery deve executar quando peça não existir")]
        [Trait("Estoque", "Application")]
        public async Task PecaExisteQuery_QuandoNaoExiste_DeveRetornarSucesso()
        {
            var query = new PecaExisteQuery(Guid.NewGuid());
            var automocker = new AutoMocker();
            var handler = automocker.CreateInstance<PecaCommandHandler>();

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Peca?)null);

            var resultado = await handler.Handle(query, CancellationToken.None);

            Assert.True(resultado.Sucesso);
        }

        [Fact(DisplayName = "Obter peças por ids deve retornar dto quando todas forem encontradas")]
        [Trait("Estoque", "Application")]
        public async Task ObterPecasPorIds_QuandoCompleto_DeveRetornarOk()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var p1 = new Peca("A", "D1", 10, 1, true, DateTime.UtcNow) { Id = id1 };
            var p2 = new Peca("B", "D2", 20, 2, true, DateTime.UtcNow) { Id = id2 };
            var cmd = new ObterPecasPorIdsCommand([id1, id2]);
            var automocker = new AutoMocker();
            automocker.Use<IMapper>(new PecaFixture().ConfigurarMapeamentoEGerarMapper());
            var handler = automocker.CreateInstance<PecaCommandHandler>();

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.Buscar(It.IsAny<Expression<Func<Peca, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Peca> { p1, p2 }.AsEnumerable());

            var resultado = await handler.Handle(cmd, CancellationToken.None);

            Assert.True(resultado.Sucesso);
            Assert.NotNull(resultado.Valor);
            Assert.Equal(2, resultado.Valor!.Count());
        }

        [Fact(DisplayName = "Obter peças por ids deve falhar quando repositório retornar nulo")]
        [Trait("Estoque", "Application")]
        public async Task ObterPecasPorIds_QuandoNulo_DeveRetornarNaoEncontrado()
        {
            var id = Guid.NewGuid();
            var cmd = new ObterPecasPorIdsCommand([id]);
            var automocker = new AutoMocker();
            automocker.Use<IMapper>(new PecaFixture().ConfigurarMapeamentoEGerarMapper());
            var handler = automocker.CreateInstance<PecaCommandHandler>();

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.Buscar(It.IsAny<Expression<Func<Peca, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<Peca>?)null!);

            var resultado = await handler.Handle(cmd, CancellationToken.None);

            Assert.False(resultado.Sucesso);
        }

        [Fact(DisplayName = "Obter peças por ids deve falhar quando faltar peça na lista")]
        [Trait("Estoque", "Application")]
        public async Task ObterPecasPorIds_QuandoContagemDivergir_DeveRetornarNaoEncontrado()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var p1 = new Peca("A", "D1", 10, 1, true, DateTime.UtcNow) { Id = id1 };
            var cmd = new ObterPecasPorIdsCommand([id1, id2]);
            var automocker = new AutoMocker();
            automocker.Use<IMapper>(new PecaFixture().ConfigurarMapeamentoEGerarMapper());
            var handler = automocker.CreateInstance<PecaCommandHandler>();

            automocker.GetMock<IPecaRepository>()
                .Setup(p => p.Buscar(It.IsAny<Expression<Func<Peca, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Peca> { p1 }.AsEnumerable());

            var resultado = await handler.Handle(cmd, CancellationToken.None);

            Assert.False(resultado.Sucesso);
        }
    }
}
