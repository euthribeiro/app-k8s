using System.Text.RegularExpressions;
using wrench.auto.repair.core.DomainObjects;

namespace wrench.auto.repair.core.ValueObjects
{
    public class Telefone
    {
        private const string TELEFONE_BRASILEIRO_PATTERN = @"^\+?55?\s?\(?\d{2}\)?\s?(9\d{4}|\d{4})-?\d{4}$";

        /// <summary>Construtor sem parametros exigido pelo Entity Framework Core para materializar o tipo a partir do banco.</summary>
        protected Telefone() { }

        public Telefone(string telefone)
        {
            var pattern = @"^(?:\+?(?<pais>55)\s?)?\(?(?<ddd>\d{2})\)?\s?(?<prefixo>9\d{4}|\d{4})-?(?<sufixo>\d{4})$";

            var match = Regex.Match(telefone, pattern);

            if (!match.Success)
                throw new DomainException("Somente telefones brasileiros são aceitos");

            var ddi = match.Groups["pais"].Value;
            var ddd = match.Groups["ddd"].Value;
            var numero = match.Groups["prefixo"].Value + match.Groups["sufixo"].Value;

            if (string.IsNullOrWhiteSpace(ddi))
                ddi = "55";

            Validar(ddi, ddd, numero);

            DDI = ddi;
            DDD = ddd;
            Numero = numero;
        }

        public Telefone(string ddd, string numero)
            : this("55", ddd, numero) { }

        public Telefone(string ddi, string ddd, string numero)
        {
            Validar(ddi, ddd, numero);

            DDI = ddi;
            DDD = ddd;
            Numero = numero;
        }

        private static void Validar(string ddi, string ddd, string numero)
        {
            Validacoes.ValidarSeVazio(ddi, "DDI não pode ser vazio");
            Validacoes.ValidarSeVazio(ddd, "DDD não pode ser vazio");
            Validacoes.ValidarSeVazio(numero, "Número não pode ser vazio");
            Validacoes.ValidarSeNaoCorrespondeAExpressaoRegular(ddi, "^[\\d]+$", "Somente números são permitidos");
            Validacoes.ValidarSeNaoCorrespondeAExpressaoRegular(ddd, "^[\\d]+$", "Somente números são permitidos");
            Validacoes.ValidarSeNaoCorrespondeAExpressaoRegular(numero, "^[\\d]+$", "Somente números são permitidos");

            var numeroCompleto = $"+{ddi}{ddd}{numero}";

            Validacoes.ValidarSeNaoCorrespondeAExpressaoRegular(numeroCompleto, TELEFONE_BRASILEIRO_PATTERN, "Somente telefones brasileiros são aceitos");
        }

        public string DDI { get; private set; }
        public string DDD { get; private set; }
        public string Numero { get; private set; }

        public string ObterTelefone()
        {
            return $"+{DDI}{DDD}{Numero}";
        }

        public override string ToString() => ObterTelefone();

        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(Telefone other) =>
            other is not null && ObterTelefone() == other.ObterTelefone();

        public override int GetHashCode()
            => ObterTelefone().GetHashCode();

        public static bool operator ==(Telefone left, Telefone right)
            => Equals(left, right);

        public static bool operator !=(Telefone left, Telefone right)
            => !Equals(left, right);
    }
}
