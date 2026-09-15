using System.Text.RegularExpressions;
using wrench.auto.repair.core.DomainObjects;
using wrench.auto.repair.core.Extensions;

namespace wrench.auto.repair.core.ValueObjects
{
    public sealed partial class NomeRazaoSocial
    {
        [GeneratedRegex(@"\s+")]
        private static partial Regex EspacosDuplicadosRegex();

        public string Nome { get; set; }

        /// <summary>Construtor sem parametros exigido pelo Entity Framework Core para materializar o tipo a partir do banco.</summary>
        protected NomeRazaoSocial() { }

        public NomeRazaoSocial(string nome)
        {
            Validar(nome);

            Nome = EspacosDuplicadosRegex()
                .Replace(nome, " ")
                .Trim()
                .RemoverAcentos()
                .ToUpper();
        }

        private static void Validar(string nome)
        {
            Validacoes.ValidarSeVazio(nome, "O nome ou razão social não pode estar vazio");

            var nomeCorrigido = EspacosDuplicadosRegex().Replace(nome, " ").Trim();

            var nomes = nomeCorrigido.Split(' ');

            Validacoes.ValidarSeIgual(nomes.Length, 1, "O nome ou razão social deve ser composto");
        }


        public override string ToString()
        {
            return Nome;
        }

        public override int GetHashCode() =>
            Nome.GetHashCode();

        public override bool Equals(object? obj) =>
            base.Equals(obj);

        public bool Equals(NomeRazaoSocial other) =>
            this.Nome == other.Nome;

        public static bool operator ==(NomeRazaoSocial left, NomeRazaoSocial right) =>
            Equals(left, right);

        public static bool operator !=(NomeRazaoSocial left, NomeRazaoSocial right) =>
            !Equals(left, right);
    }
}
