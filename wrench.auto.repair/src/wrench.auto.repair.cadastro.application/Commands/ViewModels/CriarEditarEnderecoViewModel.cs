namespace wrench.auto.repair.cadastro.application.Commands.ViewModels
{
    public class CriarEditarEnderecoViewModel
    {
        public string Logradouro { get; init; }

        public string Numero { get; init; }

        public string? Complemento { get; init; }

        public string Bairro { get; init; }

        public string Cep { get; init; }

        public string Cidade { get; init; }

        public string UnidadeFederativa { get; init; }

        public string Pais { get; set; }
    }
}
