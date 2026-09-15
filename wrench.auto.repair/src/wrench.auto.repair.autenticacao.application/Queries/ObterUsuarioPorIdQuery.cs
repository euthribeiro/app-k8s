using FluentValidation;
using wrench.auto.repair.autenticacao.application.Queries.ViewModels;
using wrench.auto.repair.core.Messages;

namespace wrench.auto.repair.autenticacao.application.Queries
{
    public class ObterUsuarioPorIdQuery(Guid id) : Command<UsuarioViewModel>
    {
        public Guid Id { get; private set; } = id;

        public override bool EhValido()
        {
            ValidationResult = new ObterUsuarioPorIdQueryValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class ObterUsuarioPorIdQueryValidator : AbstractValidator<ObterUsuarioPorIdQuery>
    {
        public static string IdUsuarioVazioError =>
            "O identificador do usuário deve ser informado";

        public ObterUsuarioPorIdQueryValidator()
        {
            RuleFor(c => c.Id)
                .NotEmpty()
                .WithMessage(IdUsuarioVazioError);
        }
    }
}
