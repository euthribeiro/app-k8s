using AutoMapper;
using MediatR;
using wrench.auto.repair.autenticacao.application.Queries.ViewModels;
using wrench.auto.repair.autenticacao.domain.Data;
using wrench.auto.repair.autenticacao.domain.Entities;
using wrench.auto.repair.core.Data;
using wrench.auto.repair.core.Errors;
using wrench.auto.repair.core.Pagination;

namespace wrench.auto.repair.autenticacao.application.Queries
{
    public class UsuarioQueryHandler(
        IMapper _mapper,
        ISortMap<Usuario> _usuarioSortMap,
        IUsuarioRepository _usuarioRepository
    ) : IRequestHandler<ObterTodosPerfisQuery, Result<IEnumerable<PerfilViewModel>>>,
        IRequestHandler<ObterTodosUsuariosQuery, Result<ResultadoPaginado<UsuarioViewModel>>>,
        IRequestHandler<ObterUsuarioPorIdQuery, Result<UsuarioViewModel>>
    {
        public async Task<Result<IEnumerable<PerfilViewModel>>> Handle(ObterTodosPerfisQuery request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result<IEnumerable<PerfilViewModel>>.UnprocessableEntity(request.ObterErros());

            var perfis = await _usuarioRepository.ObterTodosPerfisAsync(cancellationToken);

            if (perfis == null) return Result<IEnumerable<PerfilViewModel>>.NotFound("Nenhum perfil cadastrado.");

            var perfilViewModels = _mapper.Map<IEnumerable<Perfil>, IEnumerable<PerfilViewModel>>(perfis);

            return Result<IEnumerable<PerfilViewModel>>.Ok(perfilViewModels);
        }

        public async Task<Result<ResultadoPaginado<UsuarioViewModel>>> Handle(ObterTodosUsuariosQuery request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result<ResultadoPaginado<UsuarioViewModel>>.UnprocessableEntity(request.ObterErros());

            var usuarios = await _usuarioRepository
                .BuscaPaginadaAsync(request.Paginacao, _usuarioSortMap.Map, cancellationToken);

            if (usuarios == null)
                return Result<ResultadoPaginado<UsuarioViewModel>>.NotFound("Nenhum usuário cadastrado.");

            var usuarioViewModels = _mapper.Map<ResultadoPaginado<UsuarioViewModel>>(usuarios);

            return Result<ResultadoPaginado<UsuarioViewModel>>.Ok(usuarioViewModels);
        }

        public async Task<Result<UsuarioViewModel>> Handle(ObterUsuarioPorIdQuery request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result<UsuarioViewModel>.UnprocessableEntity(request.ObterErros());

            var usuario = await _usuarioRepository.ObterPorIdAsync(request.Id, cancellationToken);

            if (usuario == null)
                return Result<UsuarioViewModel>.NotFound("Usuário não encontrado");

            var usuarioViewModel = _mapper.Map<Usuario, UsuarioViewModel>(usuario);

            return Result<UsuarioViewModel>.Ok(usuarioViewModel);
        }
    }
}
