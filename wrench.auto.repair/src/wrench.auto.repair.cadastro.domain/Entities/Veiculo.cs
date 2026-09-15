using wrench.auto.repair.core.DomainObjects;
using wrench.auto.repair.core.Extensions;

namespace wrench.auto.repair.cadastro.domain.Entities
{
    public class Veiculo : Entity, IAggregateRoot
    {

        /// <summary>Construtor sem parametros exigido pelo Entity Framework Core para materializar o tipo a partir do banco.</summary>
        protected Veiculo() { }

        public Veiculo(Guid clienteId, string marca, string modelo, string cor, int anoFabricacao, int anoModelo, string placaDoVeiculo, string? descricao, DateTime? ultimaRevisao, int quilometragemAtual, DateTime dataCadastro)
        {
            Validar(clienteId, marca, modelo, cor, anoFabricacao, anoModelo, placaDoVeiculo, quilometragemAtual);

            ClienteId = clienteId;
            Marca = marca.RemoverEspacosDuplicados().RemoverAcentos().ToUpperInvariant();
            Modelo = modelo.RemoverEspacosDuplicados().RemoverAcentos().ToUpperInvariant();
            Cor = cor.RemoverEspacosDuplicados().RemoverAcentos().ToUpperInvariant();
            AnoFabricacao = anoFabricacao;
            AnoModelo = anoModelo;
            PlacaDoVeiculo = placaDoVeiculo.RemoverEspacosDuplicados().RemoverAcentos().RemoverCaracteresNaoAlfaNumericos().ToUpperInvariant();
            Descricao = descricao?.RemoverEspacosDuplicados().RemoverAcentos().ToUpperInvariant();
            UltimaRevisao = ultimaRevisao;
            QuilometragemAtual = quilometragemAtual;
            DataCadastro = dataCadastro;
        }

        public Guid ClienteId { get; private set; }
        public string Marca { get; private set; }
        public string Modelo { get; private set; }
        public string Cor { get; private set; }
        public int AnoFabricacao { get; private set; }
        public int AnoModelo { get; private set; }
        public string PlacaDoVeiculo { get; private set; }
        public string? Descricao { get; private set; }
        public DateTime? UltimaRevisao { get; private set; }
        public int QuilometragemAtual { get; private set; }
        public DateTime DataCadastro { get; private set; }
        public Cliente Cliente { get; private set; }

        public void AlterarCliente(Cliente cliente)
        {
            Validacoes.ValidarSeNulo(cliente, "O cliente não pode ser nulo");

            ClienteId = cliente.Id;
            Cliente = cliente;
        }

        public void AlterarCor(string cor)
        {
            Validacoes.ValidarSeVazio(cor, "A cor do veículo não pode ser vazio");
            Cor = cor;
        }

        public void AtualizarQuilometragem(int quilometragem)
        {
            Validacoes.ValidarSeMenorQue(quilometragem, 0, "A quilometragem não pode ser negativa");
            Validacoes.ValidarSeMenorQue(quilometragem, QuilometragemAtual, "A quilometragem não pode diminuir");

            QuilometragemAtual = quilometragem;
        }

        public void AtualizarDescricao(string? descricao)
        {
            Descricao = descricao;
        }

        public void AtualizarUltimaRevisao(DateTime ultimaRevisao)
        {
            if (UltimaRevisao.HasValue && ultimaRevisao < UltimaRevisao)
                throw new DomainException("A data da última revisão não pode ser menor que a data da revisão anterior.");

            UltimaRevisao = ultimaRevisao;
        }

        public void CorrigirMarca(string marca)
        {
            Validacoes.ValidarSeVazio(marca, "A marca do veículo não pode ser vazio");

            Marca = marca;
        }

        public void CorrigirModelo(string modelo)
        {
            Validacoes.ValidarSeVazio(modelo, "O modelo do veículo não pode ser vazio");

            Modelo = modelo;
        }

        public void CorrigirAnoFabricacao(int anoFabricacao)
        {
            var anoAtual = DateTime.UtcNow.Year;
            var proximoAno = DateTime.UtcNow.AddYears(1).Year;
            Validacoes.ValidarMinimoMaximo(anoFabricacao, 1886, proximoAno, $"Ano de fabricação deve estar entre 1886 e {anoAtual}.");

            AnoFabricacao = anoFabricacao;
        }

        public void CorrigirAnoModelo(int anoModelo)
        {
            var proximoAno = DateTime.UtcNow.AddYears(1).Year;

            Validacoes.ValidarMinimoMaximo(anoModelo, 1886, proximoAno, $"Ano do modelo deve estar entre 1886 e {proximoAno}.");

            AnoModelo = anoModelo;
        }

        public void CorrigirQuilometragem(int quilometragem)
        {
            Validacoes.ValidarSeMenorQue(quilometragem, 0, "A quilometragem atual não pode ser negativa");

            if (quilometragem > QuilometragemAtual)
                throw new DomainException("Use o método de atualizar quilometragem para aumentar o valor");

            QuilometragemAtual = quilometragem;
        }

        public void CorrigirPlacaVeiculo(string placaDoVeiculo)
        {
            Validacoes.ValidarSeVazio(placaDoVeiculo, "A placa do veículo não pode ser vazio");
            Validacoes.ValidarSeNaoCorrespondeAExpressaoRegular(placaDoVeiculo, "^[A-Z]{3}-?[0-9][0-9A-Z][0-9]{2}$", "A placa do veículo informada não é válida");

            PlacaDoVeiculo = placaDoVeiculo.Replace("-", "");
        }

        private static void Validar(Guid clienteId, string marca, string modelo, string cor, int anoFabricacao, int anoModelo, string placaVeiculo, int quilometragemAtual)
        {
            var anoAtual = DateTime.UtcNow.Year;
            var proximoAno = DateTime.UtcNow.AddYears(1).Year;

            Validacoes.ValidarSeVazio(clienteId, "O ID do cliente não pode ser vazio");
            Validacoes.ValidarSeVazio(marca, "A marca do veículo não pode ser vazio");
            Validacoes.ValidarSeVazio(modelo, "O modelo do veículo não pode ser vazio");
            Validacoes.ValidarSeVazio(cor, "A cor do veículo não pode ser vazio");
            Validacoes.ValidarMinimoMaximo(anoFabricacao, 1886, proximoAno, $"Ano de fabricação deve estar entre 1886 e {anoAtual}.");
            Validacoes.ValidarMinimoMaximo(anoModelo, 1886, proximoAno, $"Ano do modelo deve estar entre 1886 e {proximoAno}.");
            Validacoes.ValidarSeVazio(placaVeiculo, "A placa do veículo não pode ser vazio");
            Validacoes.ValidarSeNaoCorrespondeAExpressaoRegular(placaVeiculo, "^[A-Z]{3}-?[0-9][0-9A-Z][0-9]{2}$", "A placa do veículo informada não é válida");
            Validacoes.ValidarSeMenorQue(quilometragemAtual, 0, "A quilometragem atual não pode ser negativa");
        }
    }
}
