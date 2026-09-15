using wrench.auto.repair.core.DomainObjects;

namespace wrench.auto.repair.ordem.servico.domain.ValueObjects
{
    public class ItemOrdemServico
    {
        public ItemOrdemServico(Guid pecaId, string nome, decimal valorUnitario, int quantidade)
        {
            Validar(pecaId, nome, valorUnitario, quantidade);

            PecaId = pecaId;
            Nome = nome;
            ValorUnitario = valorUnitario;
            Quantidade = quantidade;
        }

        public Guid PecaId { get; private set; }
        public string Nome { get; private set; }
        public decimal ValorUnitario { get; private set; }
        public int Quantidade { get; private set; }

        public void AdicionarUnidades(int quantidade)
        {
            Validacoes.ValidarSeMenorQue(quantidade, 1, "A quantidade mínima permitida é 1");

            Quantidade += quantidade;
        }

        public decimal CalcularValorTotalPeca()
        {
            return ValorUnitario * Quantidade;
        }

        private static void Validar(Guid pecaId, string nome, decimal valorUnitario, int quantidade)
        {
            Validacoes.ValidarSeVazio(pecaId, "O identificador da peça não pode ser vazio");
            Validacoes.ValidarSeVazio(nome, "O nome da peça não pode ser vazio");
            Validacoes.ValidarSeMenorQue(valorUnitario, 0, "O valor unitário deve ser maior ou igual a zero");
            Validacoes.ValidarSeMenorQue(quantidade, 1, "A quantidade mínima permitida é 1");
        }
    }
}
