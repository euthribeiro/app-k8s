using FluentValidation;
using wrench.auto.repair.core.Messages;

namespace wrench.auto.repair.ordem.servico.application.UseCases.DiagnosticoUseCase
{
    public class RealizarDiagnosticoCommand(Guid ordemServicoId, decimal valorEstimado, string solucaoProposta, HashSet<Guid> pecasId) : Command<Guid>
    {
        public Guid OrdemServicoId { get; private set; } = ordemServicoId;
        public decimal ValorEstimado { get; private set; } = valorEstimado;
        public string SolucaoProposta { get; private set; } = solucaoProposta;
        public HashSet<Guid> PecasId { get; private set; } = pecasId;

        public override bool EhValido()
        {
            ValidationResult = new RealizarDiagnosticoCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }

        public class RealizarDiagnosticoCommandValidator : AbstractValidator<RealizarDiagnosticoCommand>
        {
            public static string PecaIdVazio =>
                "Um dos identificador das peças informadas não é válido";

            public static string MaximoPecasPermitadas =>
                "O número máximo de peças permitidas é 100";

            public RealizarDiagnosticoCommandValidator()
            {
                RuleFor(c => c.OrdemServicoId)
                    .NotEmpty()
                    .WithMessage("Ordem de serviço não informada.");

                RuleFor(c => c.SolucaoProposta)
                    .NotEmpty()
                    .WithMessage("Solução proposta não informada.");

                RuleFor(c => c.ValorEstimado)
                    .GreaterThan(0)
                    .WithMessage("Valor estimado deve ser positivo.");

                When(x => x.PecasId != null && x.PecasId.Count > 0, () =>
                {
                    RuleFor(x => x.PecasId)
                      .Must(x => x.Count <= 100)
                      .WithMessage(MaximoPecasPermitadas);

                    RuleForEach(x => x.PecasId)
                        .NotEmpty()
                        .WithMessage(PecaIdVazio);
                });
            }
        }
    }
}
