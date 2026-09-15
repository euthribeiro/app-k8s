using AutoMapper;
using wrench.auto.repair.cadastro.application.Commands.Dtos;
using wrench.auto.repair.cadastro.application.Queries.ViewModels;
using wrench.auto.repair.cadastro.domain.ValueObjects;

namespace wrench.auto.repair.cadastro.application.AutoMapper
{
    public class EnderecoProfile : Profile
    {
        public EnderecoProfile()
        {
            CreateMap<EnderecoViewModel, Endereco>().ReverseMap();
            CreateMap<EnderecoDto, Endereco>().ReverseMap();
        }
    }
}
