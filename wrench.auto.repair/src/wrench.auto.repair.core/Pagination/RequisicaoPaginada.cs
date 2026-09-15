namespace wrench.auto.repair.core.Pagination
{
    public abstract class RequisicaoPaginada
    {
        private const int TamanhoMinimoPagina = 1;
        private const int TamanhoMaximoPagina = 100;
        public readonly IEnumerable<string> _podeOrdenarPor;

        public RequisicaoPaginada(IEnumerable<string> podeOrdernarPor)
        {
            NumeroPagina = 1;
            TamanhoPagina = 10;
            OrdenarPor = null;
            Decrescente = false;
            _podeOrdenarPor = podeOrdernarPor;
        }

        public RequisicaoPaginada(
            IEnumerable<string> podeOrdernarPor,
            int numeroPagina = 1,
            int tamanhoPagina = 10,
            string? ordenarPor = null,
            bool decrescente = false
        )
        {
            NumeroPagina = numeroPagina;
            TamanhoPagina = tamanhoPagina;
            OrdenarPor = ordenarPor;
            Decrescente = decrescente;
            _podeOrdenarPor = podeOrdernarPor;
        }

        private int _numeroPagina = 1;
        public int NumeroPagina
        {
            get => _numeroPagina;
            set => _numeroPagina = value < TamanhoMinimoPagina ? TamanhoMinimoPagina : value;
        }

        private int _tamanhoPagina = 10;
        public int TamanhoPagina
        {
            get => _tamanhoPagina;
            set => _tamanhoPagina = value > TamanhoMaximoPagina ? TamanhoMaximoPagina : value;
        }

        public string? OrdenarPor { get; set; }

        public bool Decrescente { get; set; } = false;
    }
}
