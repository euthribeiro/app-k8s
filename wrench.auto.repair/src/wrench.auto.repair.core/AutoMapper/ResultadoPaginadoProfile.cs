using AutoMapper;
using wrench.auto.repair.core.Pagination;

namespace wrench.auto.repair.core.AutoMapper
{
    public class ResultadoPaginadoProfile : Profile
    {
        public ResultadoPaginadoProfile()
        {
            CreateMap(typeof(ResultadoPaginado<>), typeof(ResultadoPaginado<>))
                .ConvertUsing(typeof(ResultadoPaginadoConverter<,>));
        }
    }

    public class ResultadoPaginadoConverter<TOrigem, TDestino>
    : ITypeConverter<ResultadoPaginado<TOrigem>, ResultadoPaginado<TDestino>>
    {
        public ResultadoPaginado<TDestino> Convert(
            ResultadoPaginado<TOrigem> source,
            ResultadoPaginado<TDestino> destination,
            ResolutionContext context)
        {
            var itens = context.Mapper.Map<IEnumerable<TDestino>>(source.Itens);

            return new ResultadoPaginado<TDestino>(
                itens,
                source.TotalRegistros,
                source.NumeroPagina,
                source.TamanhoPagina
            );
        }
    }
}
