using FluentValidation;
using wrench.auto.repair.autenticacao.application.Validators;
using wrench.auto.repair.core.Messages;

namespace wrench.auto.repair.autenticacao.application.Commands
{
    public class PrimeiroAcessoUsuarioCommand(string email, string senha) : Command
    {
        public string Email { get; private set; } = email;

        public string Senha { get; private set; } = senha;

        public override bool EhValido()
        {
            ValidationResult = new PrimeiroAcessoUsuarioCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class PrimeiroAcessoUsuarioCommandValidator : AbstractValidator<PrimeiroAcessoUsuarioCommand>
    {
        public static readonly string EmailInvalidoErro = "O e-mail informado não é válido";
        public static readonly string EmailVazioErro = "E-mail não pode ser vazio";

        public PrimeiroAcessoUsuarioCommandValidator()
        {
            RuleFor(c => c.Email)
                .NotEmpty()
                .WithMessage(EmailInvalidoErro)
                .EmailAddress()
                .WithMessage(EmailVazioErro);

            RuleFor(c => c.Senha)
                .SetValidator(new PasswordValidator(string.Empty, null, null));
        }
    }
}
