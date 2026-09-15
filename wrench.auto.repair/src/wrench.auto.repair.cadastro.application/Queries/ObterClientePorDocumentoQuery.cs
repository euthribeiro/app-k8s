using DocumentValidator;
using FluentValidation;
using System.Text.RegularExpressions;
using wrench.auto.repair.cadastro.application.Queries.ViewModels;
using wrench.auto.repair.core.Messages;

namespace wrench.auto.repair.cadastro.application.Queries
{
    public class ObterClientePorDocumentoQuery(string documento) : Command<ClienteViewModel>
    {
        public string Documento { get; private set; } = documento;

        public override bool EhValido()
        {
            ValidationResult = new ObterClientePorDocumentoQueryValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public partial class ObterClientePorDocumentoQueryValidator : AbstractValidator<ObterClientePorDocumentoQuery>
    {
        [GeneratedRegex(@"\D")]
        private static partial Regex RemoverNaoNumericosRegex();

        public static string DocumentoVazioError =>
            "O CPF/CNPJ do cliente não pode ser vazio";

        public static string DocumentoInvalidoError =>
            "O CPF/CNPJ informado não é válido";

        public ObterClientePorDocumentoQueryValidator()
        {
            RuleFor(c => c.Documento)
             .NotEmpty()
             .WithMessage(DocumentoVazioError)
             .Must(SerDocumentoValido)
             .WithMessage(DocumentoInvalidoError);
        }

        public static bool SerDocumentoValido(string documento)
        {
            var doc = RemoverNaoNumericosRegex().Replace(documento, "");

            if (doc.Length > 15) return false;

            if (doc.Length <= 11) return CpfValidation.Validate(doc);

            return CnpjValidation.Validate(doc);
        }


    }
}
