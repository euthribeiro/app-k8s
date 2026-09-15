using AutoMapper;
using System.ComponentModel;
using wrench.auto.repair.core.Extensions;
using wrench.auto.repair.ordem.servico.application.Queries.ViewModels;
using wrench.auto.repair.ordem.servico.domain.Entities;
using wrench.auto.repair.ordem.servico.domain.ValueObjects;

namespace wrench.auto.repair.ordem.servico.application.AutoMapper
{
    public class OrdemServicoProfile : Profile
    {
        public OrdemServicoProfile()
        {
            CreateMap<OrdemServico, OrdemServicoViewModel>()
                .ForMember(o => o.Status, opt => opt.MapFrom(src => src.Status.GetAttribute<DescriptionAttribute>().Description))
                .ForMember(o => o.StatusAprovacao, opt => opt.MapFrom(src => src.StatusAprovacao.HasValue ? src.StatusAprovacao.GetAttribute<DescriptionAttribute>().Description : null))
                .ForMember(o => o.ValorTotal, opt => opt.MapFrom(src => src.CalcularValorTotal()))
                .ForMember(o => o.Pecas, opt => opt.MapFrom(src => src.Pecas.ToList()))
                .ForMember(o => o.TempoExecucaoOrdem, opt => opt.MapFrom(src => src.CalculaTempoExecucaoOrdem()));

            CreateMap<ItemOrdemServico, ItemOrdemServicoViewModel>();
        }
    }
}
