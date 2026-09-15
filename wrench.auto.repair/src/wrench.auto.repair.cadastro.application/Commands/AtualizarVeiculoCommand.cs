using FluentValidation;
using System.Text.RegularExpressions;
using wrench.auto.repair.core.Messages;

namespace wrench.auto.repair.cadastro.application.Commands
{
    public class AtualizarVeiculoCommand(
        Guid veiculoId,
        Guid clienteId,
        string marca,
        string modelo,
        string cor,
        int anoFabricacao,
        int anoModelo,
        string placaDoVeiculo,
        string? descricao,
        DateTime? ultimaRevisao,
        int quilometragemAtual
    ) : Command
    {
        public Guid VeiculoId { get; private set; } = veiculoId;
        public Guid ClienteId { get; private set; } = clienteId;
        public string Marca { get; private set; } = marca;
        public string Modelo { get; private set; } = modelo;
        public string Cor { get; set; } = cor;
        public int AnoFabricacao { get; private set; } = anoFabricacao;
        public int AnoModelo { get; private set; } = anoModelo;
        public string PlacaDoVeiculo { get; private set; } = placaDoVeiculo;
        public string? Descricao { get; private set; } = descricao;
        public DateTime? UltimaRevisao { get; set; } = ultimaRevisao;
        public int QuilometragemAtual { get; set; } = quilometragemAtual;

        public string ObterPlacaVeiculoSemHifens()
        {
            return PlacaDoVeiculo.Replace("-", "");
        }

        public override bool EhValido()
        {
            ValidationResult = new AtualizarVeiculoCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public partial class AtualizarVeiculoCommandValidator : AbstractValidator<AtualizarVeiculoCommand>
    {
        [GeneratedRegex("^[A-Z]{3}-?[0-9][0-9A-Z][0-9]{2}$")]
        private static partial Regex PlacaVeiculoValidoRegex();

        private static int TAMANHO_MINIMO_COR => 3;
        private static int TAMANHO_MAXIMO_COR => 30;
        private static int MENOR_ANO_FABRICACAO_MODELO => 1886;
        private static int MAIOR_ANO_FABRICACAO => DateTime.UtcNow.Year;
        private static int MAIOR_ANO_MODELO => DateTime.UtcNow.Year + 1;
        private static int QUILOMETRAGEM_MINIMA => 0;

        public static string VeiculoIdVazioError =>
            "O ID do veículo deve ser informado";

        public static string ClienteIdVazioError =>
            "O ID do cliente dono do veículo deve ser informado";
        public static string MarcaVaziaError =>
            "A marca do carro deve ser informado";

        public static string ModeloVazioError =>
            "O modelo do carro deve ser informado";

        public static string CorVazioError =>
            "A cor do veículo deve ser informado";

        public static string TamanhoMinimoCorError =>
            $"A cor do veículo deve ter no mínimo {TAMANHO_MINIMO_COR} caracteres";

        public static string TamanhoMaximoCorError =>
            $"A cor do veículo deve ter no mínimo {TAMANHO_MAXIMO_COR} caracteres";

        public static string MenorAnoDeFabricacaoError =>
            $"A ano de fabricação de veículo não pode ser menor que {MENOR_ANO_FABRICACAO_MODELO}";

        public static string MaiorAnoDeFabricacaoError =>
            $"A ano de fabricação de veículo não pode ser maior que {MAIOR_ANO_FABRICACAO}";

        public static string MenorAnoModeloError =>
            $"A ano do modelo de veículo não pode ser menor que {MENOR_ANO_FABRICACAO_MODELO}";

        public static string MaiorAnoModeloError =>
            $"A ano do modelo de veículo não pode ser maior que {MAIOR_ANO_MODELO}";

        public static string PlacaVeiculoVazioError =>
            "A placa do veículo deve ser informada";

        public static string PlacaVeiculoInvalidoError =>
            "A placa do veículo informado não é válido";

        public static string QuilometragemInvalidaError =>
            $"A quilometragem não pode ser menor que {QUILOMETRAGEM_MINIMA}";

        public AtualizarVeiculoCommandValidator()
        {
            RuleFor(c => c.VeiculoId)
              .NotEmpty()
              .WithMessage(VeiculoIdVazioError);

            RuleFor(c => c.ClienteId)
                .NotEmpty()
                .WithMessage(ClienteIdVazioError);

            RuleFor(c => c.Marca)
                .NotEmpty()
                .WithMessage(MarcaVaziaError);

            RuleFor(c => c.Modelo)
                .NotEmpty()
                .WithMessage(ModeloVazioError);

            RuleFor(c => c.Cor)
                .NotEmpty()
                .WithMessage(CorVazioError)
                .MinimumLength(TAMANHO_MINIMO_COR)
                .WithMessage(TamanhoMinimoCorError)
                .MaximumLength(TAMANHO_MAXIMO_COR)
                .WithMessage(TamanhoMaximoCorError);

            RuleFor(c => c.AnoFabricacao)
                .GreaterThanOrEqualTo(MENOR_ANO_FABRICACAO_MODELO)
                .WithMessage(MenorAnoDeFabricacaoError)
                .LessThanOrEqualTo(MAIOR_ANO_FABRICACAO)
                .WithMessage(MaiorAnoDeFabricacaoError);

            RuleFor(c => c.AnoModelo)
                .GreaterThanOrEqualTo(MENOR_ANO_FABRICACAO_MODELO)
                .WithMessage(MenorAnoModeloError)
                .LessThanOrEqualTo(MAIOR_ANO_FABRICACAO)
                .WithMessage(MaiorAnoModeloError);

            RuleFor(c => c.PlacaDoVeiculo)
                .NotEmpty()
                .WithMessage(PlacaVeiculoVazioError)
                .Must(placa => PlacaVeiculoValidoRegex().IsMatch(placa))
                .WithMessage(PlacaVeiculoInvalidoError);

            RuleFor(c => c.QuilometragemAtual)
                .GreaterThanOrEqualTo(QUILOMETRAGEM_MINIMA)
                .WithMessage(QuilometragemInvalidaError);
        }
    }
}
