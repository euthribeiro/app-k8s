using FluentValidation;
using System.Text.RegularExpressions;
using wrench.auto.repair.cadastro.application.Commands.Dtos;
using wrench.auto.repair.core.Messages;

namespace wrench.auto.repair.cadastro.application.Commands
{
    public class AtualizarClienteCommand(
        Guid clienteId,
        string nome,
        string telefone,
        string email,
        EnderecoDto? endereco = null
    ) : Command
    {
        public Guid ClienteId { get; private set; } = clienteId;
        public string Nome { get; private set; } = nome;
        public string Telefone { get; private set; } = telefone;
        public string Email { get; private set; } = email;
        public EnderecoDto? Endereco { get; private set; } = endereco;

        public override bool EhValido()
        {
            ValidationResult = new AtualizarClienteCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public partial class AtualizarClienteCommandValidator : AbstractValidator<AtualizarClienteCommand>
    {
        [GeneratedRegex(@"\s+")]
        private static partial Regex EspacosEmBrancoRegex();

        [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "pt-BR")]
        private static partial Regex EmailRegex();

        [GeneratedRegex(@"^(?:\+?(?<pais>55)\s?)?\(?(?<ddd>\d{2})\)?\s?(?<prefixo>9\d{4}|\d{4})-?(?<sufixo>\d{4})$")]
        private static partial Regex TelefoneBrasileiroRegex();

        public static string ClienteIdError =>
            "O identificador do cliente deve ser informado";

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

        public AtualizarClienteCommandValidator()
        {
            RuleFor(c => c.ClienteId)
                .NotEmpty()
                .WithMessage(ClienteIdError);

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

            When(c => c.Endereco is not null, () =>
            {
                RuleFor(c => c.Endereco!)
                .SetValidator(new EnderecoDtoValidator());
            });
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
