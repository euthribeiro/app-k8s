using FluentValidation;
using wrench.auto.repair.autenticacao.application.Validators;
using wrench.auto.repair.core.Messages;

namespace wrench.auto.repair.autenticacao.application.Commands
{
    public class CriarUsuarioCommand(string email, string? senha, Guid perfilId, bool ativo, bool requerSenha = true) : Command<Guid>
    {
        public string Email { get; private set; } = email;
        public string? Senha { get; private set; } = senha;
        public Guid PerfilId { get; private set; } = perfilId;
        public bool Ativo { get; private set; } = ativo;
        public bool RequerSenha { get; private set; } = requerSenha;

        public override bool EhValido()
        {
            ValidationResult = new CriarUsuarioCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class CriarUsuarioCommandValidator : AbstractValidator<CriarUsuarioCommand>
    {
        public static readonly string EmailInvalidoErro = "O e-mail informado não é válido";
        public static readonly string EmailVazioErro = "E-mail não pode ser vazio";
        public static readonly string PerfilVazioErro = "O Perfil não pode ser vazio";

        public CriarUsuarioCommandValidator()
        {
            RuleFor(c => c.Email)
                .NotEmpty()
                .WithMessage(EmailInvalidoErro)
                .EmailAddress()
                .WithMessage(EmailVazioErro);

            RuleFor(c => c.PerfilId)
                .NotEmpty()
                .WithMessage(PerfilVazioErro);

            When(c => c.RequerSenha, () =>
            {
                RuleFor(c => c.Senha!)
                .SetValidator(new PasswordValidator(string.Empty, null, null));
            });
        }
    }
}
