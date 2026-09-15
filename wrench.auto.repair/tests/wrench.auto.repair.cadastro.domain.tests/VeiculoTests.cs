using wrench.auto.repair.cadastro.domain.Entities;
using wrench.auto.repair.cadastro.domain.tests.Fixtures;
using wrench.auto.repair.core.DomainObjects;

namespace wrench.auto.repair.cadastro.domain.tests
{
    public class VeiculoTests(VeiculoFixture _veiculoFixture, ClienteFixture _clienteFixture) :
        IClassFixture<VeiculoFixture>, IClassFixture<ClienteFixture>
    {
        [Fact(DisplayName = "Criar Novo Veículo Dados Vazio e Invalido Deve Retornar Exception")]
        [Trait("Cadastro", "Domains")]
        public void CriarNovoVeiculo_DadosVazioEInvalidos_DeveRetornarException()
        {
            var placa = _veiculoFixture.GerarPlacaVeiculo();

            Assert.Throws<DomainException>(() =>
            {
                new Veiculo(Guid.Empty, "Fiat", "Toro", "Preto", 2018, 2019, placa, null, null, 0, DateTime.UtcNow);
            });

            Assert.Throws<DomainException>(() =>
            {
                new Veiculo(Guid.NewGuid(), "", "Toro", "Preto", 2018, 2019, placa, null, null, 0, DateTime.UtcNow);
            });

            Assert.Throws<DomainException>(() =>
            {
                new Veiculo(Guid.NewGuid(), "Fiat", "", "Preto", 2018, 2019, placa, null, null, 0, DateTime.UtcNow);
            });

            Assert.Throws<DomainException>(() =>
            {
                new Veiculo(Guid.NewGuid(), "Fiat", "Toro", "", 0, 2019, placa, null, null, 0, DateTime.UtcNow);
            });

            Assert.Throws<DomainException>(() =>
            {
                new Veiculo(Guid.NewGuid(), "Fiat", "Toro", "Preto", 0, 2019, placa, null, null, 0, DateTime.UtcNow);
            });

            Assert.Throws<DomainException>(() =>
            {
                new Veiculo(Guid.NewGuid(), "Fiat", "Toro", "Preto", 2018, 0, "", null, null, 0, DateTime.UtcNow);
            });

            Assert.Throws<DomainException>(() =>
            {
                new Veiculo(Guid.NewGuid(), "Fiat", "Toro", "Preto", 2018, 0, "GGG", null, null, 0, DateTime.UtcNow);
            });
        }

        [Fact(DisplayName = "Criar Novo Veículo Valido Com Sucesso")]
        [Trait("Cadastro", "Domains")]
        public void CriarNovoVeiculo_VeiculoValido_DeveCriarComSucesso()
        {
            var placa = _veiculoFixture.GerarPlacaVeiculo();

            var veiculo = new Veiculo(Guid.NewGuid(), "Fiat", "Toro", "Preto", 2018, 2019, placa, null, DateTime.UtcNow, 100000, DateTime.UtcNow);

            Assert.NotNull(veiculo);
        }

        [Fact(DisplayName = "Veiculo Alterar Cliente Nulo Deve Retornar Exception")]
        [Trait("Cadastro", "Domains")]
        public void Veiculo_AlterarClienteNulo_DeveRetornarException()
        {
            var veiculo = _veiculoFixture.CriarVeiculoValido();

            Assert.Throws<DomainException>(() => veiculo.AlterarCliente(null));
        }

        [Fact(DisplayName = "Veiculo Atualizar Cliente Valido Deve Atualizar")]
        [Trait("Cadastro", "Domains")]
        public void Veiculo_AlterClienteValido_DeveAtualizar()
        {
            var veiculo = _veiculoFixture.CriarVeiculoValido();
            var cliente = _clienteFixture.GerarClienteValido();

            veiculo.AlterarCliente(cliente);

            Assert.Equal(cliente, veiculo.Cliente);
            Assert.Equal(cliente.Id, veiculo.ClienteId);
        }

        [Fact(DisplayName = "Veiculo Atualizar Cor Vazio Deve Retornar Exception")]
        [Trait("Cadastro", "Domains")]
        public void Veiculo_AtualizarCorVazio_DeveRetornarException()
        {
            var veiculo = _veiculoFixture.CriarVeiculoValido();

            Assert.Throws<DomainException>(() => veiculo.AlterarCor(""));
        }

        [Fact(DisplayName = "Veiculo Atualizar Quilometragem Negativa Deve Retornar Exception")]
        [Trait("Cadastro", "Domains")]
        public void Veiculo_AtualizarQuilometragemNegativa_DeveRetornarException()
        {
            var veiculo = _veiculoFixture.CriarVeiculoValido();

            Assert.Throws<DomainException>(() => veiculo.AtualizarQuilometragem(-1));
        }

        [Fact(DisplayName = "Veiculo Atualizar Quilometragem Menor Que Anterior Deve Retornar Exception")]
        [Trait("Cadastro", "Domains")]
        public void Veiculo_AtualizarQuilometragemMenorQueAnterior_DeveRetornarException()
        {
            var veiculo = _veiculoFixture.CriarVeiculoValido(km: 10000);

            Assert.Throws<DomainException>(() => veiculo.AtualizarQuilometragem(9999));
        }

        [Fact(DisplayName = "Veiculo Atualizar Quilometragem Válida")]
        [Trait("Cadastro", "Domains")]
        public void Veiculo_AtualizarQuilometragemValidar_DeveAtualizar()
        {
            var veiculo = _veiculoFixture.CriarVeiculoValido(km: 10000);

            veiculo.AtualizarQuilometragem(10001);

            Assert.Equal(10001, veiculo.QuilometragemAtual);
        }

        [Fact(DisplayName = "Veiculo Atualizar Descrição")]
        [Trait("Cadastro", "Domains")]
        public void Veiculo_AtualizarDescricao_DeveAtualizar()
        {
            var descricao = "Descrição do Veículo";
            var veiculo = _veiculoFixture.CriarVeiculoValido(km: 10000);

            veiculo.AtualizarDescricao(descricao);

            Assert.Equal(descricao, veiculo.Descricao);
        }

        [Fact(DisplayName = "Veiculo Atualizar Última Revisão Data Anterior Deve Retornar Exception")]
        [Trait("Cadastro", "Domains")]
        public void AtualizarUltimaRevisao_DataAnterior_DeveRetornarException()
        {
            var veiculo = _veiculoFixture.CriarVeiculoValido(ultimaRevisao: DateTime.UtcNow);

            Assert.Throws<DomainException>(() => veiculo.AtualizarUltimaRevisao(DateTime.UtcNow.AddDays(-1)));
        }

        [Fact(DisplayName = "Veiculo Atualizar Última Revisão Com Sucesso")]
        [Trait("Cadastro", "Domains")]
        public void AtualizarUltimaRevisao_DataValida_DeveAtualizarComSucesso()
        {
            var veiculo = _veiculoFixture.CriarVeiculoValido(ultimaRevisao: DateTime.UtcNow.AddDays(-1));
            var dataNovaRevisao = DateTime.UtcNow;

            veiculo.AtualizarUltimaRevisao(dataNovaRevisao);

            Assert.Equal(veiculo.UltimaRevisao, dataNovaRevisao);
        }

        [Fact(DisplayName = "Corrigir Marca Veiculo Marca Vazia Deve Retornar Exception")]
        [Trait("Cadastro", "Domains")]
        public void CorrigirMarcaVeiculo_MarcaVazia_DeveRetornarException()
        {
            var veiculo = _veiculoFixture.CriarVeiculoValido();

            Assert.Throws<DomainException>(() => veiculo.CorrigirMarca(""));
        }

        [Fact(DisplayName = "Corrigir Marca Veiculo Marca Valida")]
        [Trait("Cadastro", "Domains")]
        public void CorrigirMarcaVeiculo_MarcaValida_DeveCorrigir()
        {
            var veiculo = _veiculoFixture.CriarVeiculoValido();
            var novaMarca = "Chevrolet";

            veiculo.CorrigirMarca(novaMarca);

            Assert.Equal(novaMarca, veiculo.Marca);
        }

        [Fact(DisplayName = "Corrigir Modelo Veiculo Modelo Vazia Deve Retornar Exception")]
        [Trait("Cadastro", "Domains")]
        public void CorrigirModeloVeiculo_ModeloVazia_DeveRetornarException()
        {
            var veiculo = _veiculoFixture.CriarVeiculoValido();

            Assert.Throws<DomainException>(() => veiculo.CorrigirModelo(""));
        }

        [Fact(DisplayName = "Corrigir Modelo Veiculo Modelo Valida")]
        [Trait("Cadastro", "Domains")]
        public void CorrigirModeloVeiculo_ModeloValido_DeveCorrigir()
        {
            var veiculo = _veiculoFixture.CriarVeiculoValido();
            var novoModelo = "Prisma";

            veiculo.CorrigirModelo(novoModelo);

            Assert.Equal(novoModelo, veiculo.Modelo);
        }

        [Fact(DisplayName = "Corrigir Veiculo Ano Fabricação Inválido")]
        [Trait("Cadastro", "Domains")]
        public void CorrigirVeiculo_AnoFabricacaoInvalido_DeveRetornarException()
        {
            var veiculo = _veiculoFixture.CriarVeiculoValido();

            Assert.Throws<DomainException>(() => veiculo.CorrigirAnoFabricacao(0));
        }

        [Fact(DisplayName = "Corrigir Ano Fabricação Válido")]
        [Trait("Cadastro", "Domains")]
        public void CorrigirVeiculo_AnoFabricacao_DeveCorrigir()
        {
            var veiculo = _veiculoFixture.CriarVeiculoValido();
            var novoAno = 2015;

            veiculo.CorrigirAnoFabricacao(novoAno);

            Assert.Equal(novoAno, veiculo.AnoFabricacao);
        }

        [Fact(DisplayName = "Corrigir Veiculo Ano Modelo Inválido")]
        [Trait("Cadastro", "Domains")]
        public void CorrigirVeiculo_AnoModeloInvalido_DeveRetornarException()
        {
            var veiculo = _veiculoFixture.CriarVeiculoValido();

            Assert.Throws<DomainException>(() => veiculo.CorrigirAnoModelo(0));
        }

        [Fact(DisplayName = "Corrigir Ano Modelo Válido")]
        [Trait("Cadastro", "Domains")]
        public void CorrigirVeiculo_AnoModelo_DeveCorrigir()
        {
            var veiculo = _veiculoFixture.CriarVeiculoValido();
            var novoAno = 2015;

            veiculo.CorrigirAnoModelo(novoAno);

            Assert.Equal(novoAno, veiculo.AnoModelo);
        }

        [Fact(DisplayName = "Corrigir Veiculo Quilometragem Inválido")]
        [Trait("Cadastro", "Domains")]
        public void CorrigirVeiculo_QuilometragemInvalida_DeveRetornarException()
        {
            var veiculo = _veiculoFixture.CriarVeiculoValido();

            Assert.Throws<DomainException>(() => veiculo.CorrigirQuilometragem(-1));
        }

        [Fact(DisplayName = "Corrigir Veiculo Quilometragem Maior Que Anterior Deve Retornar Exception")]
        [Trait("Cadastro", "Domains")]
        public void CorrigirVeiculo_QuilometragemMaiorQueAnterior_DeveRetornarException()
        {
            var veiculo = _veiculoFixture.CriarVeiculoValido(km: 50);
            var novaKm = 60;

            Assert.Throws<DomainException>(() => veiculo.CorrigirQuilometragem(novaKm));
        }

        [Fact(DisplayName = "Corrigir Quilometragem Menor Válido")]
        [Trait("Cadastro", "Domains")]
        public void CorrigirVeiculo_CorrigirQuilometragemValorMenor_DeveCorrigir()
        {
            var veiculo = _veiculoFixture.CriarVeiculoValido(km: 50);
            var novaQuilometragem = 40;

            veiculo.CorrigirQuilometragem(novaQuilometragem);

            Assert.Equal(novaQuilometragem, veiculo.QuilometragemAtual);
        }

        [Fact(DisplayName = "Corrigir Placa Veiculo")]
        [Trait("Cadastro", "Domains")]
        public void CorrigirVeiculo_PlacaVazia_DeveRetornarException()
        {
            var veiculo = _veiculoFixture.CriarVeiculoValido();

            Assert.Throws<DomainException>(() => veiculo.CorrigirPlacaVeiculo(""));
        }

        [Fact(DisplayName = "Corrigir Placa Inválida")]
        [Trait("Cadastro", "Domains")]
        public void CorrigirVeiculo_PlacaInvalida_DeveRetornarException()
        {
            var veiculo = _veiculoFixture.CriarVeiculoValido();

            Assert.Throws<DomainException>(() => veiculo.CorrigirPlacaVeiculo("GGG"));
        }

        [Fact(DisplayName = "Corrigir Placa Válida")]
        [Trait("Cadastro", "Domains")]
        public void CorrigirVeiculo_PlacaValida_DeveCorrigir()
        {
            var veiculo = _veiculoFixture.CriarVeiculoValido();
            var novaPlaca = _veiculoFixture.GerarPlacaVeiculo();

            veiculo.CorrigirPlacaVeiculo(novaPlaca);

            Assert.Equal(novaPlaca.Replace("-", ""), veiculo.PlacaDoVeiculo);
        }
    }
}
