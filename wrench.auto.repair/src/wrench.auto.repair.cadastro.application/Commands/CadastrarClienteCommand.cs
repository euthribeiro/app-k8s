using DocumentValidator;
using FluentValidation;
using System.Text.RegularExpressions;
using wrench.auto.repair.cadastro.application.Commands.Dtos;
using wrench.auto.repair.cadastro.application.Commands.ViewModels;
using wrench.auto.repair.core.Messages;

namespace wrench.auto.repair.cadastro.application.Commands
{
    public class CadastrarClienteCommand(
        string documento,
        string nome,
        string telefone,
        string email,
        EnderecoDto endereco
        ) : Command<Guid>
    {
        public string Documento { get; private set; } = documento;
        public string Nome { get; private set; } = nome;
        public string Telefone { get; private set; } = telefone;
        public string Email { get; private set; } = email;
        public EnderecoDto Endereco { get; private set; } = endereco;

        public override bool EhValido()
        {
            ValidationResult = new CadastrarClienteCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }

        public static implicit operator CadastrarClienteCommand(CadastrarClienteViewModel viewModel)
        {
            return new CadastrarClienteCommand(
                viewModel.Documento,
                viewModel.Nome,
                viewModel.Telefone,
                viewModel.Email,
                viewModel.Endereco
            );
        }
    }

    public partial class CadastrarClienteCommandValidator : AbstractValidator<CadastrarClienteCommand>
    {
        [GeneratedRegex(@"\D")]
        private static partial Regex RemoverNaoNumericosRegex();

        [GeneratedRegex(@"\s+")]
        private static partial Regex EspacosEmBrancoRegex();

        [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "pt-BR")]
        private static partial Regex EmailRegex();

        [GeneratedRegex(@"^(?:\+?(?<pais>55)\s?)?\(?(?<ddd>\d{2})\)?\s?(?<prefixo>9\d{4}|\d{4})-?(?<sufixo>\d{4})$")]
        private static partial Regex TelefoneBrasileiroRegex();

        public static string DocumentoVazioError =>
            "O CPF/CNPJ do cliente não pode ser vazio";

        public static string DocumentoInvalidoError =>
            "O CPF/CNPJ informado não é válido";

        public static string NomeVazioError =>
            "O nome do cliente não pode ser vazio";

        public static string NomeInvalidoError =>
            "O sobrenome deve ser informado.";

        public static string TelefoneVazioError =>
            "O telefone não pode ser vazio.";

        public static string TelefoneInvalidoError =>
            "Telefone inválido. Use o formato brasileiro.";

        public static string EmailVazioError =>
            "O e-mail não pode ser vazio.";

        public static string EmailInvalidoError =>
            "E-mail inválido.";

        public static string EnderecoNuloError =>
            "O endereço deve ser informado.";

        public CadastrarClienteCommandValidator()
        {
            RuleFor(c => c.Documento)
                .NotEmpty()
                .WithMessage(DocumentoVazioError)
                .Must(SerDocumentoValido)
                .WithMessage(DocumentoInvalidoError);

            RuleFor(c => c.Nome)
                .NotEmpty()
                .WithMessage(NomeVazioError)
                .Must(SerNomeComposto)
                .WithMessage(NomeInvalidoError);

            RuleFor(c => c.Telefone)
              .NotEmpty()
              .WithMessage(TelefoneVazioError)
              .Must(SerTelefoneValido)
              .WithMessage(TelefoneInvalidoError);

            RuleFor(c => c.Email)
                .NotEmpty()
                .WithMessage(EmailVazioError)
                .Must(SerEmailValido)
                .WithMessage(EmailInvalidoError);

            RuleFor(c => c.Endereco)
                .NotNull()
                .WithMessage(EnderecoNuloError)
                .SetValidator(new EnderecoDtoValidator());
        }

        public static bool SerDocumentoValido(string documento)
        {
            var doc = RemoverNaoNumericosRegex().Replace(documento, "");

            if (doc.Length > 15) return false;

            if (doc.Length <= 11) return CpfValidation.Validate(doc);

            return CnpjValidation.Validate(doc);
        }

        public static bool SerNomeComposto(string nome)
        {
            return EspacosEmBrancoRegex().Replace(nome, " ").Trim().Split(' ').Length > 1;
        }

        public static bool SerTelefoneValido(string telefone)
        {
            return TelefoneBrasileiroRegex().IsMatch(telefone);
        }

        public static bool SerEmailValido(string email)
        {
            return EmailRegex().IsMatch(email);
        }
    }
}
