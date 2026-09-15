using System.Net.Mail;
using System.Text.RegularExpressions;
using wrench.auto.repair.core.DomainObjects;
using wrench.auto.repair.core.Extensions;

namespace wrench.auto.repair.core.ValueObjects
{
    public partial class Email
    {
        [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "pt-BR")]
        private static partial Regex EmailRegex();

        /// <summary>Construtor sem parametros exigido pelo Entity Framework Core para materializar o tipo a partir do banco.</summary>
        protected Email() { }

        public string Endereco { get; private set; }

        public string Dominio { get; private set; }

        public Email(string value)
        {
            Validar(value);

            Endereco = value.RemoverAcentos().Trim().ToLowerInvariant();
            Dominio = Endereco.Split('@')[1].ToLowerInvariant();
        }

        private static void Validar(string value)
        {
            Validacoes.ValidarSeVazio(value, "E-mail não pode ser vazio");
            Validacoes.ValidarSeNaoCorrespondeAExpressaoRegular(value, EmailRegex(), "E-mail inválido");
            if (!EhValido(value)) throw new DomainException("E-mail inválido");
        }

        public override string ToString() => Endereco;

        public override bool Equals(object? obj)
        {
            return obj is Email other && Equals(other);
        }

        public bool Equals(Email other) =>
            other is not null && Endereco == other.Endereco;

        public override int GetHashCode()
            => Endereco.GetHashCode();

        public static bool operator ==(Email left, Email right)
            => Equals(left, right);

        public static bool operator !=(Email left, Email right)
            => !Equals(left, right);

        public static bool EhValido(string email)
        {
            try
            {
                var mail = new MailAddress(email);
                return mail.Address == email;
            }
            catch
            {
                return false;
            }
        }


    }
}
