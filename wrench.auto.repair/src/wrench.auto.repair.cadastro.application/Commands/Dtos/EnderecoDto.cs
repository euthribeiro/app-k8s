using FluentValidation;
using wrench.auto.repair.cadastro.application.Commands.ViewModels;

namespace wrench.auto.repair.cadastro.application.Commands.Dtos
{
    public class EnderecoDto(
        string logradouro,
        string numero,
        string? complemento,
        string bairro,
        string cep,
        string cidade,
        string unidadeFederativa,
        string pais = "Brasil"
    )
    {
        public string Logradouro { get; private set; } = logradouro;

        public string Numero { get; private set; } = numero;

        public string? Complemento { get; private set; } = complemento;

        public string Bairro { get; private set; } = bairro;

        public string Cep { get; private set; } = cep;

        public string Cidade { get; private set; } = cidade;

        public string UnidadeFederativa { get; private set; } = unidadeFederativa;

        public string Pais { get; private set; } = pais;

        public static implicit operator EnderecoDto(CriarEditarEnderecoViewModel viewModel)
        {
            return new EnderecoDto(
                viewModel.Logradouro,
                viewModel.Numero,
                viewModel.Complemento,
                viewModel.Bairro,
                viewModel.Cep,
                viewModel.Cidade,
                viewModel.UnidadeFederativa,
                viewModel.Pais
            );
        }
    }

    public class EnderecoDtoValidator : AbstractValidator<EnderecoDto>
    {
        public static string LogradouroVazioError =>
            "O logradouro não pode ser vazio";

        public static string NumeroVazioError =>
            "O número não pode ser vazio";

        public static string BairroVazioError =>
            "O bairro não pode ser vazio";

        public static string CepVazioError =>
            "O CEP não pode ser vazio";

        public static string CepInvalidoError =>
            "O CEP informado não é válido";

        public static string CidadeVaziaError =>
            "A cidade não pode ser vazio";

        public static string UnidadeFederativaVazioError =>
            "A unidade federativa não pode ser vazio";

        public static string UnidadeFederativaInvalidaError =>
            "Unidade Federativa inválida. Informe apenas o valor abreviado. Exemplo: SP";

        public static string PaisVazioError =>
         "País não pode ser vazio";

        public EnderecoDtoValidator()
        {
            RuleFor(e => e.Logradouro)
                .NotEmpty()
                .WithMessage(LogradouroVazioError);

            RuleFor(e => e.Numero)
                .NotEmpty()
                .WithMessage(NumeroVazioError);

            RuleFor(e => e.Bairro)
                .NotEmpty()
                .WithMessage(BairroVazioError);

            RuleFor(e => e.Cep)
                .NotEmpty()
                .WithMessage(CepVazioError)
                .Matches("^\\d{5}-?\\d{3}$")
                .WithMessage(CepInvalidoError);

            RuleFor(e => e.Cidade)
                .NotEmpty()
                .WithMessage(CidadeVaziaError);


            RuleFor(e => e.UnidadeFederativa)
                .NotEmpty()
                .WithMessage(UnidadeFederativaVazioError)
                .Matches(@"^\w{2}$")
                .WithMessage(UnidadeFederativaInvalidaError);

            RuleFor(e => e.Pais)
                 .NotEmpty()
                 .WithMessage(PaisVazioError);
        }
    }
}
