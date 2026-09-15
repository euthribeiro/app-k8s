using MediatR;
using wrench.auto.repair.autenticacao.domain.Data;
using wrench.auto.repair.autenticacao.domain.Entities;
using wrench.auto.repair.autenticacao.domain.Models;
using wrench.auto.repair.autenticacao.domain.Security;
using wrench.auto.repair.autenticacao.infra.Security;
using wrench.auto.repair.core.Errors;
using wrench.auto.repair.core.ValueObjects;

namespace wrench.auto.repair.autenticacao.application.Commands
{
    public class UsuarioCommandHandler(
        IPasswordHasher _passwordHasher,
        IUsuarioRepository _usuarioRepository,
        IJwtTokenGenerator _jwtTokenGenerator
    ) : IRequestHandler<CriarUsuarioCommand, Result<Guid>>,
        IRequestHandler<AutenticarUsuarioCommand, Result<TokenAcesso>>,
        IRequestHandler<AtivarUsuarioCommand, Result>,
        IRequestHandler<InativarUsuarioCommand, Result>,
        IRequestHandler<PrimeiroAcessoUsuarioCommand, Result>,
        IRequestHandler<ResetarSenhaUsuarioCommand, Result>
    {
        public async Task<Result<Guid>> Handle(CriarUsuarioCommand request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result<Guid>.ValidationError(request.ObterErros());

            var usuario = await _usuarioRepository
                .ObterPorEmailAsync(new Email(request.Email), cancellationToken);

            if (usuario != null) return Result<Guid>.ValidationError("Usuário já cadastrado");

            var perfil = await _usuarioRepository.ObterPerfilPorIdAsync(request.PerfilId);

            if (perfil == null) return Result<Guid>.ValidationError("Perfil não encontrado");

            var novoUsuario = new Usuario(new Email(request.Email), perfil.Id, request.Ativo, DateTime.UtcNow);

            if (!string.IsNullOrWhiteSpace(request.Senha))
            {
                var hash = _passwordHasher.GerarHash(request.Senha);
                novoUsuario.DefinirSenha(hash);
            }

            await _usuarioRepository.Adicionar(novoUsuario, cancellationToken);

            var salvo = await _usuarioRepository.UnitOfWork.CommitAsync();

            if (!salvo) return Result<Guid>.Unexpected("Não foi possível cadastrar o usuário, por favor tente novamente.");

            return Result<Guid>.Created(novoUsuario.Id);
        }

        public async Task<Result<TokenAcesso>> Handle(AutenticarUsuarioCommand request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result<TokenAcesso>.ValidationError(request.ObterErros());

            var usuario = await _usuarioRepository
                .ObterPorEmailAsync(new Email(request.Email), cancellationToken);

            if (usuario == null || !usuario.Ativo)
                return Result<TokenAcesso>.Unauthorized("Usuário não autorizado.");

            if (string.IsNullOrWhiteSpace(usuario.Senha))
                return Result<TokenAcesso>.Unauthorized("Usuário sem senha cadastrada");

            var senhaValida = _passwordHasher.ValidarSenha(request.Senha, usuario.Senha);

            if (!senhaValida) return Result<TokenAcesso>.Unauthorized("Usuário não autorizado.");

            var tokenAccess = _jwtTokenGenerator.GerarToken(usuario);

            return Result<TokenAcesso>.Ok(tokenAccess);
        }

        public async Task<Result> Handle(InativarUsuarioCommand request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result.ValidationError(request.ObterErros());

            var usuario = await _usuarioRepository
                .ObterPorIdAsync(request.UsuarioId, cancellationToken);

            if (usuario == null) return Result.NotFound("Usuário não encontrado");

            if (!usuario.Ativo)
                return Result.NoContent();

            if (usuario.Perfil.Nome == "Admin")
                return Result.Conflicted("Usuário adminstrador não pode ser inativado");

            usuario.Inativar();

            await _usuarioRepository.Atualizar(usuario);

            var alteracoesSalvas = await _usuarioRepository.UnitOfWork.CommitAsync();

            if (!alteracoesSalvas)
                return Result.Unexpected("Não foi possível inativar o usuário. Por favor tente novamente.");

            return Result.NoContent();
        }

        public async Task<Result> Handle(PrimeiroAcessoUsuarioCommand request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result.ValidationError(request.ObterErros());

            var usuario = await _usuarioRepository
                .ObterPorEmailAsync(new Email(request.Email), cancellationToken);

            if (usuario == null || !usuario.Ativo) return Result.NotFound("Usuário não encontrado");

            if (!string.IsNullOrWhiteSpace(usuario.Senha))
                return Result.Forbidden("Não permitido.");

            if (usuario.Perfil.Nome == "Admin")
                return Result.Conflicted("Senha do usuário adminstrador não pode ser alterado por esse serviço.");

            var passwordHash = _passwordHasher.GerarHash(request.Senha);

            usuario.DefinirSenha(passwordHash);

            await _usuarioRepository.Atualizar(usuario);

            var alteracoesSalvas = await _usuarioRepository.UnitOfWork.CommitAsync();

            if (!alteracoesSalvas)
                return Result.Unexpected("Não foi possível alterar a senha do usuário. Por favor tente novamente.");

            return Result.NoContent();
        }

        public async Task<Result> Handle(ResetarSenhaUsuarioCommand request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result.ValidationError(request.ObterErros());

            var usuario = await _usuarioRepository
                .ObterPorIdAsync(request.UsuarioId, cancellationToken);

            if (usuario == null || !usuario.Ativo) return Result.NotFound("Usuário não encontrado");

            if (string.IsNullOrWhiteSpace(usuario.Senha))
                return Result.NoContent();

            if (usuario.Perfil.Nome == "Admin")
                return Result.Conflicted("Senha do usuário adminstrador não pode ser resetada.");

            usuario.ResetarSenha();

            await _usuarioRepository.Atualizar(usuario);

            var alteracoesSalvas = await _usuarioRepository.UnitOfWork.CommitAsync();

            if (!alteracoesSalvas)
                return Result.Unexpected("Não foi possível alterar a senha do usuário. Por favor tente novamente.");

            return Result.NoContent();
        }

        public async Task<Result> Handle(AtivarUsuarioCommand request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result.ValidationError(request.ObterErros());

            var usuario = await _usuarioRepository
                .ObterPorIdAsync(request.UsuarioId, cancellationToken);

            if (usuario == null) return Result.NotFound("Usuário não encontrado");

            if (usuario.Ativo)
                return Result.NoContent();

            usuario.Ativar();

            await _usuarioRepository.Atualizar(usuario);

            var alteracoesSalvas = await _usuarioRepository.UnitOfWork.CommitAsync();

            if (!alteracoesSalvas)
                return Result.Unexpected("Não foi possível ativar o usuário. Por favor tente novamente.");

            return Result.NoContent();
        }
    }
}
