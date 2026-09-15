using wrench.auto.repair.cadastro.application.Commands.ViewModels;

namespace wrench.auto.repair.cadastro.application.tests.Commands.ViewModels
{
    public class CommandViewModelsSmokeTests
    {
        [Fact(DisplayName = "View models de comando de cadastro devem aceitar valores mínimos")]
        [Trait("Cadastro", "Application")]
        public void ViewModels_DevemExporPropriedades()
        {
            var endereco = new CriarEditarEnderecoViewModel
            {
                Logradouro = "Rua A",
                Numero = "10",
                Complemento = null,
                Bairro = "Centro",
                Cep = "01310100",
                Cidade = "São Paulo",
                UnidadeFederativa = "SP"
            };
            Assert.Equal("Rua A", endereco.Logradouro);

            var cadCliente = new CadastrarClienteViewModel
            {
                Documento = "12345678901234",
                Nome = "Empresa X",
                Telefone = "11 98888-7777",
                Email = "x@empresa.com",
                Endereco = endereco
            };
            Assert.Equal("Empresa X", cadCliente.Nome);

            var atualCliente = new AtualizarClienteViewModel
            {
                ClienteId = Guid.NewGuid(),
                Nome = "Nome Atualizado",
                Telefone = "11 97777-6666",
                Email = "y@empresa.com",
                Endereco = null
            };
            Assert.Null(atualCliente.Endereco);

            var cadVeiculo = new CadastrarVeiculoViewModel
            {
                ClienteId = Guid.NewGuid(),
                Marca = "FORD",
                Modelo = "KA",
                Cor = "Branco",
                AnoFabricacao = 2020,
                AnoModelo = 2021,
                PlacaDoVeiculo = "ABC1D23",
                Descricao = null,
                UltimaRevisao = null,
                QuilometragemAtual = 10000
            };
            Assert.Equal("FORD", cadVeiculo.Marca);

            var atualVeiculo = new AtualizarVeiculoViewModel
            {
                VeiculoId = Guid.NewGuid(),
                ClienteId = Guid.NewGuid(),
                Marca = "VW",
                Modelo = "GOL",
                Cor = "Preto",
                AnoFabricacao = 2019,
                AnoModelo = 2020,
                PlacaDoVeiculo = "XYZ9W87",
                Descricao = "OK",
                UltimaRevisao = DateTime.UtcNow,
                QuilometragemAtual = 50000
            };
            Assert.Equal("GOL", atualVeiculo.Modelo);
        }
    }
}
