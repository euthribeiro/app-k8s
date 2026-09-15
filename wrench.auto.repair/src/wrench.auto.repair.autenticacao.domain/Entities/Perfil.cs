using wrench.auto.repair.core.DomainObjects;

namespace wrench.auto.repair.autenticacao.domain.Entities
{
    public class Perfil : Entity
    {
        /// <summary>Construtor sem parametros exigido pelo Entity Framework Core para materializar o tipo a partir do banco.</summary>
        private Perfil() { }

        public Perfil(string nome, string descricao, bool ativo, DateTime dataCriacao)
        {
            Nome = nome;
            Descricao = descricao;
            Ativo = ativo;
            DataCriacao = dataCriacao;

            Validar();
        }

        public string Nome { get; private set; }
        public string Descricao { get; private set; }
        public bool Ativo { get; set; }
        public DateTime DataCriacao { get; private set; }

        /// <summary>Propriedade de navegacao mapeada pelo Entity Framework Core.</summary>
        public ICollection<Usuario> Usuarios { get; set; } = [];

        private void Validar()
        {
            Validacoes.ValidarSeVazio(Nome, "O nome não pode ser vazio");
            Validacoes.ValidarSeVazio(Descricao, "A descrição não pode ser vazio");
        }

        public void AlterarDescricao(string descricao)
        {
            Validacoes.ValidarSeVazio(descricao, "A descrição não pode ser vazio");
            Descricao = descricao;
        }
    }
}
