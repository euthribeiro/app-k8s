using wrench.auto.repair.cadastro.application.Commands.ViewModels;

namespace wrench.web.api.Models.OrdemServico
{
    public class NovaOrdemServicoRequest
    {
        public string Descricao { get; set; }
        public CadastrarClienteViewModel Cliente { get; set; }

        public CadastrarVeiculoViewModel Veiculo { get; set; }
    }
}
