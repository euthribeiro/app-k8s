using AutoMapper;
using wrench.auto.repair.cadastro.application.Queries.ViewModels;
using wrench.auto.repair.cadastro.domain.Entities;

namespace wrench.auto.repair.cadastro.application.AutoMapper
{
    public class ClienteProfile : Profile
    {
        public ClienteProfile()
        {
            CreateMap<Cliente, ClienteViewModel>()
                .ForMember(c => c.Documento, opt => opt.MapFrom(src => src.Documento.Numeracao))
                .ForMember(c => c.Nome, opt => opt.MapFrom(src => src.Nome.Nome))
                .ForMember(c => c.Telefone, opt => opt.MapFrom(src => src.Telefone.ObterTelefone()))
                .ForMember(c => c.Email, opt => opt.MapFrom(src => src.Email.Endereco));
        }
    }
}
