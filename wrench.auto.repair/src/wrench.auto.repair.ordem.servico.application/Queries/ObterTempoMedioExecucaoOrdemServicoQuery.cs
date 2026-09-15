using FluentValidation;
using wrench.auto.repair.core.Messages;
using wrench.auto.repair.ordem.servico.application.Queries.ViewModels;

namespace wrench.auto.repair.ordem.servico.application.Queries
{
    public class ObterTempoMedioExecucaoOrdemServicoQuery : Command<MonitoramentoViewModel>
    {
        public override bool EhValido()
        {
            ValidationResult = new ObterTempoMedioExecucaoOrdemServicoQueryValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class ObterTempoMedioExecucaoOrdemServicoQueryValidator : AbstractValidator<ObterTempoMedioExecucaoOrdemServicoQuery>
    {
        public ObterTempoMedioExecucaoOrdemServicoQueryValidator()
        {

        }
    }
}
