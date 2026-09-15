namespace wrench.auto.repair.core.Pagination
{
    public class ResultadoPaginado<T>(
        IEnumerable<T> itens,
        int totalRegistros,
        int numeroPagina,
        int tamanhoPagina,
        IEnumerable<string>? ordenacoesPermitidas = null
    )
    {
        public IEnumerable<T> Itens { get; } = itens;
        public int TotalRegistros { get; } = totalRegistros;
        public int NumeroPagina { get; } = numeroPagina;
        public int TamanhoPagina { get; } = tamanhoPagina;

        public int TotalPaginas => (int)Math.Ceiling((double)TotalRegistros / TamanhoPagina);
        public IEnumerable<string>? OrdenacoesPermitidas { get; } = ordenacoesPermitidas;
    }
}
