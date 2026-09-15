using AutoMapper;
using wrench.auto.repair.autenticacao.application.Queries.ViewModels;
using wrench.auto.repair.autenticacao.domain.Entities;

namespace wrench.auto.repair.autenticacao.application.AutoMapper
{
    public class UsuarioProfile : Profile
    {
        public UsuarioProfile()
        {
            CreateMap<Usuario, UsuarioViewModel>()
                .ForMember(u => u.Email, opt => opt.MapFrom(src => src.Email.Endereco));
        }
    }
}
