using wrench.auto.repair.cadastro.domain.Entities;
using wrench.auto.repair.core.DomainObjects;
using wrench.auto.repair.core.Extensions;

namespace wrench.auto.repair.cadastro.domain.ValueObjects
{
    public class Endereco
    {
        protected Endereco() { }

        public Endereco(string logradouro, string numero, string? complemento, string bairro, string cep, string cidade, string unidadeFederativa, string pais)
        {
            Validar(logradouro, numero, bairro, cep, cidade, unidadeFederativa, pais);

            Logradouro = logradouro.Trim().ToUpperInvariant().RemoverAcentos();
            Numero = numero.Trim().ToUpperInvariant().RemoverAcentos();
            Complemento = complemento?.Trim().ToUpperInvariant().RemoverAcentos();
            Bairro = bairro.Trim().ToUpperInvariant().RemoverAcentos();
            Cep = cep.Replace("-", "").Trim();
            Cidade = cidade.Trim().ToUpperInvariant().RemoverAcentos();
            UnidadeFederativa = unidadeFederativa.Trim().ToUpperInvariant().RemoverAcentos();
            Pais = pais.Trim().ToUpperInvariant().RemoverAcentos();
        }

        private static void Validar(string logradouro, string numero, string bairro, string cep, string cidade, string unidadeFederativa, string pais)
        {
            Validacoes.ValidarSeVazio(logradouro, "Logradouro não pode ser vazio");
            Validacoes.ValidarSeVazio(numero, "Número não pode ser vazio");
            Validacoes.ValidarSeVazio(bairro, "Bairro não pode ser vazio");
            Validacoes.ValidarSeVazio(cep, "CEP não pode ser vazio");
            Validacoes.ValidarSeNaoCorrespondeAExpressaoRegular(cep, @"^\d{5}-?\d{3}$", "CEP inválido.");
            Validacoes.ValidarMinimoMaximo(cep.RemoverCaracteresNaoNumericos().Length, 8, 8, "CEP deve ter exatamente 8 caracteres");
            Validacoes.ValidarSeVazio(cidade, "Cidade não pode ser vazio");
            Validacoes.ValidarSeVazio(unidadeFederativa, "Unidade Federativa não pode ser vazio");
            Validacoes.ValidarSeNaoCorrespondeAExpressaoRegular(unidadeFederativa, @"^\w{2}$", "A unidade federativa deve conter apenas 2 caracteres.");
            Validacoes.ValidarSeVazio(pais, "País não pode ser vazio");
        }

        public string Logradouro { get; private set; }

        public string Numero { get; private set; }

        public string? Complemento { get; private set; }

        public string Bairro { get; private set; }

        public string Cep { get; private set; }

        public string Cidade { get; private set; }

        public string UnidadeFederativa { get; private set; }

        public string Pais { get; private set; }

        /// <summary>Propriedade de navegacao mapeada pelo Entity Framework Core.</summary>
        public Cliente Cliente { get; private set; }

        public string ObterEnderecoFormatado()
        {
            var cepFormatado = Cep?.Length == 8
                ? $"{Cep[..5]}-{Cep.Substring(5, 3)}"
                : Cep;

            var complemento = string.IsNullOrWhiteSpace(Complemento)
                ? string.Empty
                : $" - {Complemento}";

            return $"{Logradouro}, {Numero}{complemento}, {Bairro}, {Cidade} - {UnidadeFederativa}, CEP: {cepFormatado}, {Pais}";
        }

        public override string ToString() => ObterEnderecoFormatado();

        public bool Equals(Endereco other) =>
            other is not null &&
            ObterEnderecoFormatado()
            .Equals(other.ObterEnderecoFormatado(), StringComparison.InvariantCultureIgnoreCase);

        public override int GetHashCode()
            => ObterEnderecoFormatado().GetHashCode();

        public static bool operator ==(Endereco left, Endereco right)
            => Equals(left, right);

        public static bool operator !=(Endereco left, Endereco right)
            => !Equals(left, right);
    }
}
