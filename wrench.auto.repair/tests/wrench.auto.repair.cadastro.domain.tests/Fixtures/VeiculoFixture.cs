using Bogus;
using Fare;
using wrench.auto.repair.cadastro.domain.Entities;

namespace wrench.auto.repair.cadastro.domain.tests.Fixtures
{

    [CollectionDefinition(nameof(VeiculoCollection))]
    public class VeiculoCollection : ICollectionFixture<VeiculoFixture>
    {

    }

    public class VeiculoFixture
    {

        private static readonly string[] items = new[] {
            "Vermelho", "Azul", "Verde", "Amarelo",
            "Preto", "Branco", "Roxo", "Laranja"
        };

        public string GerarPlacaVeiculo()
        {
            return new Xeger("^[A-Z]{3}-?[0-9][0-9A-Z][0-9]{2}$", new Random()).Generate();
        }

        public Veiculo CriarVeiculoValido(bool zeroKm = false, int? km = null, DateTime? ultimaRevisao = null)
        {
            string corAleatoria = GerarCorAleatoria();
            var placa = GerarPlacaVeiculo();

            return new Faker<Veiculo>()
                .CustomInstantiator(fk => new Veiculo(
                    Guid.NewGuid(),
                    fk.Vehicle.Manufacturer(),
                    fk.Vehicle.Model(),
                    corAleatoria,
                    fk.Random.Number(1886, DateTime.UtcNow.Year),
                    fk.Random.Number(1886, DateTime.UtcNow.Year + 1),
                    placa,
                    null,
                    ultimaRevisao,
                    zeroKm ? 0 : (km ?? fk.Random.Number(0, 200000)),
                    DateTime.UtcNow
                ));
        }

        public static string GerarCorAleatoria()
        {
            var faker = new Faker();

            return faker.PickRandom(items);
        }
    }
}
