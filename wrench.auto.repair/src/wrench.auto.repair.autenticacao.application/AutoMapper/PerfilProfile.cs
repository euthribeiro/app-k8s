using AutoMapper;
using wrench.auto.repair.autenticacao.application.Queries.ViewModels;
using wrench.auto.repair.autenticacao.domain.Entities;

namespace wrench.auto.repair.autenticacao.application.AutoMapper
{
    public class PerfilProfile : Profile
    {
        public PerfilProfile()
        {
            CreateMap<Perfil, PerfilViewModel>();
        }
    }
}
