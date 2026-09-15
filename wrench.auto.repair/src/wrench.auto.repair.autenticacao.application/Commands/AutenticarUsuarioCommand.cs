using FluentValidation;
using wrench.auto.repair.autenticacao.domain.Models;
using wrench.auto.repair.core.Messages;

namespace wrench.auto.repair.autenticacao.application.Commands
{
    public class AutenticarUsuarioCommand(string email, string senha) : Command<TokenAcesso>
    {
        public string Email { get; protected set; } = email;
        public string Senha { get; protected set; } = senha;

        public override bool EhValido()
        {
            ValidationResult = new AutenticarUsuarioCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class AutenticarUsuarioCommandValidator : AbstractValidator<AutenticarUsuarioCommand>
    {
        public static readonly string EmailInvalidoErro = "O e-mail informado não é válido";
        public static readonly string EmailVazioErro = "E-mail não pode ser vazio";
        public static readonly string SenhaVazia = "A senha deve ser informada";

        public AutenticarUsuarioCommandValidator()
        {
            RuleFor(c => c.Email)
                .NotEmpty()
                .WithMessage(EmailInvalidoErro)
                .EmailAddress()
                .WithMessage(EmailVazioErro);

            RuleFor(c => c.Senha!)
             .NotEmpty()
             .WithMessage(SenhaVazia);
        }
    }
}
