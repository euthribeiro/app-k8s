using DocumentValidator;
using System.Text.RegularExpressions;
using wrench.auto.repair.core.DomainObjects;

namespace wrench.auto.repair.core.ValueObjects
{
    public class CpfCnpj
    {
        /// <summary>Construtor sem parametros exigido pelo Entity Framework Core para materializar o tipo a partir do banco.</summary>
        protected CpfCnpj() { }

        public string Numeracao { get; set; }
        public TipoDocumentoEnum TipoDocumento { get; private set; }

        public CpfCnpj(string documento)
        {
            Validar(documento);

            Numeracao = RemoverPontuacao(documento);
            TipoDocumento = ObterTipoDocumento(Numeracao);
        }

        public bool EhValido()
        {
            if (TipoDocumento == TipoDocumentoEnum.Cpf)
                return CpfValidation.Validate(Numeracao);

            return CnpjValidation.Validate(Numeracao);
        }

        public string ObterDocumentoFormatado()
        {
            if (TipoDocumento == TipoDocumentoEnum.Cpf)
                return FormatarCpf(Numeracao);
            return FormatarCnpj(Numeracao);
        }

        private void Validar(string documento)
        {
            Validacoes.ValidarSeVazio(documento, "Documento não pode ser vazio");

            var documentoSemPontuacao = RemoverPontuacao(documento);

            if (ObterTipoDocumento(documentoSemPontuacao) == TipoDocumentoEnum.Cpf)
            {
                if (!CpfValidation.Validate(documentoSemPontuacao))
                    throw new DomainException("Documento inválido");
            }
            else
            {
                if (!CnpjValidation.Validate(documentoSemPontuacao))
                    throw new DomainException("Documento inválido");
            }
        }

        private TipoDocumentoEnum ObterTipoDocumento(string documento)
        {
            return RemoverPontuacao(documento).Length <= 11 ? TipoDocumentoEnum.Cpf : TipoDocumentoEnum.Cnpj;
        }

        private static string RemoverPontuacao(string documento)
        {
            return Regex.Replace(documento, "\\D", "");
        }

        private static string FormatarCpf(string documento)
        {
            if (string.IsNullOrWhiteSpace(documento))
                return documento;

            var cpf = Regex.Replace(documento, @"\D", "");

            if (cpf.Length != 11)
                throw new ArgumentException("CPF inválido");

            return Regex.Replace(cpf, @"(\d{3})(\d{3})(\d{3})(\d{2})", "$1.$2.$3-$4");
        }

        public static string FormatarCnpj(string documento)
        {
            if (string.IsNullOrWhiteSpace(documento))
                return documento;

            var cnpj = Regex.Replace(documento, @"\D", "");

            if (cnpj.Length != 14)
                throw new ArgumentException("CNPJ inválido");

            return Regex.Replace(cnpj, @"(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})", "$1.$2.$3/$4-$5");
        }

        public override string ToString() => ObterDocumentoFormatado();

        public bool Equals(CpfCnpj other) =>
            other is not null && Numeracao == other.Numeracao;

        public override int GetHashCode()
            => Numeracao.GetHashCode();

        public static bool operator ==(CpfCnpj left, CpfCnpj right)
            => Equals(left, right);

        public static bool operator !=(CpfCnpj left, CpfCnpj right)
            => !Equals(left, right);
    }
}
