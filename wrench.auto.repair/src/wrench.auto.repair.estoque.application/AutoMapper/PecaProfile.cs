using AutoMapper;
using wrench.auto.repair.core.Messages.CommonMessages.IntegratedQueries.Dtos;
using wrench.auto.repair.estoque.application.Queries.ViewModels;
using wrench.auto.repair.estoque.domain.Entities;

namespace wrench.auto.repair.estoque.application.AutoMapper
{
    public class PecaProfile : Profile
    {
        public PecaProfile()
        {
            CreateMap<Peca, PecaViewModel>();
            CreateMap<Peca, PecaDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.Valor, o => o.MapFrom(s => (decimal)s.Valor));
        }
    }
}
