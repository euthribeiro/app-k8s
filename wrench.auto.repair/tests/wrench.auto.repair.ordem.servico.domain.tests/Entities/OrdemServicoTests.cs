using wrench.auto.repair.core.DomainObjects;
using wrench.auto.repair.ordem.servico.domain.Entities;
using wrench.auto.repair.ordem.servico.domain.Enums;
using wrench.auto.repair.ordem.servico.domain.ValueObjects;

namespace wrench.auto.repair.ordem.servico.domain.tests.Entities
{
    public class OrdemServicoTests
    {
        [Fact(DisplayName = "OrdemServico deve normalizar descrição ao criar")]
        [Trait("Ordem Serviço", "Domain")]
        public void Construtor_DeveNormalizarDescricao()
        {
            var clienteId = Guid.NewGuid();
            var veiculoId = Guid.NewGuid();
            var data = new DateTime(2024, 3, 15, 12, 0, 0, DateTimeKind.Utc);

            var ordem = new OrdemServico(clienteId, veiculoId, "  barulho  na  roda  ", OrdemServicoStatus.Recebida, data);

            Assert.Equal(" BARULHO NA RODA ", ordem.Descricao);
            Assert.Equal(clienteId, ordem.ClienteId);
            Assert.Equal(veiculoId, ordem.VeiculoId);
            Assert.Equal(OrdemServicoStatus.Recebida, ordem.Status);
        }

        [Fact(DisplayName = "OrdemServico deve falhar quando cliente for vazio")]
        [Trait("Ordem Serviço", "Domain")]
        public void Construtor_DeveLancar_QuandoClienteIdInvalido()
        {
            Assert.Throws<DomainException>(() =>
                new OrdemServico(Guid.Empty, Guid.NewGuid(), "Serviço", OrdemServicoStatus.Recebida, DateTime.UtcNow));
        }

        [Fact(DisplayName = "OrdemServico deve falhar quando veículo for vazio")]
        [Trait("Ordem Serviço", "Domain")]
        public void Construtor_DeveLancar_QuandoVeiculoIdInvalido()
        {
            Assert.Throws<DomainException>(() =>
                new OrdemServico(Guid.NewGuid(), Guid.Empty, "Serviço", OrdemServicoStatus.Recebida, DateTime.UtcNow));
        }

        [Fact(DisplayName = "OrdemServico deve falhar quando descrição for vazia")]
        [Trait("Ordem Serviço", "Domain")]
        public void Construtor_DeveLancar_QuandoDescricaoVazia()
        {
            Assert.Throws<DomainException>(() =>
                new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "   ", OrdemServicoStatus.Recebida, DateTime.UtcNow));
        }

        [Fact(DisplayName = "SolicitaDiagnostico deve alterar status quando ordem recebida")]
        [Trait("Ordem Serviço", "Domain")]
        public void SolicitaDiagnostico_DeveAlterarStatus_QuandoRecebida()
        {
            var ordem = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "Motor falhando", OrdemServicoStatus.Recebida, DateTime.UtcNow);

            ordem.SolicitaDiagnostico();

            Assert.Equal(OrdemServicoStatus.EmDiagnostico, ordem.Status);
        }

        [Fact(DisplayName = "SolicitaDiagnostico deve falhar quando status não for recebida")]
        [Trait("Ordem Serviço", "Domain")]
        public void SolicitaDiagnostico_DeveLancar_QuandoStatusInvalido()
        {
            var ordem = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "Freio", OrdemServicoStatus.EmDiagnostico, DateTime.UtcNow);

            Assert.Throws<DomainException>(() => ordem.SolicitaDiagnostico());
        }

        [Fact(DisplayName = "AdicionarDiagnostico deve atualizar valores em diagnóstico")]
        [Trait("Ordem Serviço", "Domain")]
        public void AdicionarDiagnostico_DeveAtualizarOrdem_QuandoEmDiagnostico()
        {
            var ordem = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "Óleo", OrdemServicoStatus.EmDiagnostico, DateTime.UtcNow);

            ordem.AdicionarDiagnostico("Troca de filtro e óleo", 350m);

            Assert.Equal(350m, ordem.ValorServico);
            Assert.Equal(OrdemServicoStatus.AguardandoAprovacao, ordem.Status);
            Assert.Equal(StatusAprovacaoEnum.EmAnalise, ordem.StatusAprovacao);
        }

        [Fact(DisplayName = "AdicionarDiagnostico deve falhar quando não estiver em diagnóstico")]
        [Trait("Ordem Serviço", "Domain")]
        public void AdicionarDiagnostico_DeveLancar_QuandoStatusInvalido()
        {
            var ordem = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "Pneu", OrdemServicoStatus.Recebida, DateTime.UtcNow);

            Assert.Throws<DomainException>(() => ordem.AdicionarDiagnostico("Troca", 100m));
        }

        [Fact(DisplayName = "AdicionarDiagnostico deve falhar quando solução for vazia")]
        [Trait("Ordem Serviço", "Domain")]
        public void AdicionarDiagnostico_DeveLancar_QuandoSolucaoVazia()
        {
            var ordem = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "Ar", OrdemServicoStatus.EmDiagnostico, DateTime.UtcNow);

            Assert.Throws<DomainException>(() => ordem.AdicionarDiagnostico("  ", 50m));
        }

        [Fact(DisplayName = "AdicionarDiagnostico deve falhar quando valor for negativo")]
        [Trait("Ordem Serviço", "Domain")]
        public void AdicionarDiagnostico_DeveLancar_QuandoValorNegativo()
        {
            var ordem = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "Bateria", OrdemServicoStatus.EmDiagnostico, DateTime.UtcNow);

            Assert.Throws<DomainException>(() => ordem.AdicionarDiagnostico("Troca", -1m));
        }

        [Fact(DisplayName = "AprovarOrcamento deve colocar ordem em execução")]
        [Trait("Ordem Serviço", "Domain")]
        public void AprovarOrcamento_DeveAtualizarStatus()
        {
            var ordem = CriarOrdemAguardandoAprovacao();

            ordem.AprovarOrcamento();

            Assert.Equal(OrdemServicoStatus.EmExecucao, ordem.Status);
            Assert.Equal(StatusAprovacaoEnum.Aprovada, ordem.StatusAprovacao);
        }

        [Fact(DisplayName = "AprovarOrcamento deve falhar quando não estiver aguardando aprovação")]
        [Trait("Ordem Serviço", "Domain")]
        public void AprovarOrcamento_DeveLancar_QuandoStatusInvalido()
        {
            var ordem = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "Serviço", OrdemServicoStatus.Recebida, DateTime.UtcNow);

            Assert.Throws<DomainException>(() => ordem.AprovarOrcamento());
        }

        [Fact(DisplayName = "RecusarOrcamento deve finalizar ordem")]
        [Trait("Ordem Serviço", "Domain")]
        public void RecusarOrcamento_DeveFinalizar()
        {
            var ordem = CriarOrdemAguardandoAprovacao();

            ordem.RecusarOrcamento("Valor alto");

            Assert.Equal(OrdemServicoStatus.Finalizada, ordem.Status);
            Assert.Equal(StatusAprovacaoEnum.Recusada, ordem.StatusAprovacao);
            Assert.Equal("Valor alto", ordem.MotivoRecusa);
        }

        [Fact(DisplayName = "RecusarOrcamento deve falhar quando motivo for vazio")]
        [Trait("Ordem Serviço", "Domain")]
        public void RecusarOrcamento_DeveLancar_QuandoMotivoVazio()
        {
            var ordem = CriarOrdemAguardandoAprovacao();

            Assert.Throws<DomainException>(() => ordem.RecusarOrcamento("  "));
        }

        [Fact(DisplayName = "FinalizarOrdemServico deve finalizar quando em execução")]
        [Trait("Ordem Serviço", "Domain")]
        public void FinalizarOrdemServico_DeveFinalizar_QuandoEmExecucao()
        {
            var ordem = CriarOrdemAguardandoAprovacao();
            ordem.AprovarOrcamento();

            ordem.FinalizarOrdemServico();

            Assert.Equal(OrdemServicoStatus.Finalizada, ordem.Status);
        }

        [Fact(DisplayName = "FinalizarOrdemServico deve falhar quando não estiver em execução")]
        [Trait("Ordem Serviço", "Domain")]
        public void FinalizarOrdemServico_DeveLancar_QuandoStatusInvalido()
        {
            var ordem = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "Serviço", OrdemServicoStatus.Recebida, DateTime.UtcNow);

            Assert.Throws<DomainException>(() => ordem.FinalizarOrdemServico());
        }

        [Fact(DisplayName = "EntregarServico deve marcar entregue quando finalizada")]
        [Trait("Ordem Serviço", "Domain")]
        public void EntregarServico_DeveMarcarEntregue_QuandoFinalizada()
        {
            var ordem = CriarOrdemAguardandoAprovacao();
            ordem.AprovarOrcamento();
            ordem.FinalizarOrdemServico();

            ordem.EntregarServico();

            Assert.Equal(OrdemServicoStatus.Entregue, ordem.Status);
            Assert.NotNull(ordem.DataEntrega);
        }

        [Fact(DisplayName = "EntregarServico deve falhar quando ordem não estiver finalizada")]
        [Trait("Ordem Serviço", "Domain")]
        public void EntregarServico_DeveLancar_QuandoNaoFinalizada()
        {
            var ordem = CriarOrdemAguardandoAprovacao();

            Assert.Throws<DomainException>(() => ordem.EntregarServico());
        }

        [Fact(DisplayName = "CalcularValorTotal deve somar serviço e peças")]
        [Trait("Ordem Serviço", "Domain")]
        public void CalcularValorTotal_DeveSomarPecasEValorServico()
        {
            var ordem = CriarOrdemAguardandoAprovacao();
            ordem.AprovarOrcamento();

            var peca1 = new ItemOrdemServico(Guid.NewGuid(), "Filtro", 25m, 2);
            var peca2 = new ItemOrdemServico(Guid.NewGuid(), "Óleo", 40m, 1);
            ordem.AdicionarPeca(peca1);
            ordem.AdicionarPeca(peca2);

            var esperado = ordem.ValorServico + peca1.CalcularValorTotalPeca() + peca2.CalcularValorTotalPeca();

            Assert.Equal(esperado, ordem.CalcularValorTotal());
        }

        private static OrdemServico CriarOrdemAguardandoAprovacao()
        {
            var ordem = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "Revisão", OrdemServicoStatus.EmDiagnostico, DateTime.UtcNow);
            ordem.AdicionarDiagnostico("Pacote revisão completa", 500m);
            return ordem;
        }
    }
}
