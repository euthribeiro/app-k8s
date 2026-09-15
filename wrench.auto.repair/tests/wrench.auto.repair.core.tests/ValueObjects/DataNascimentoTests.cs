using wrench.auto.repair.core.DomainObjects;
using wrench.auto.repair.core.ValueObjects;

namespace wrench.auto.repair.core.tests.ValueObjects
{
    public class DataNascimentoTests
    {
        [Fact(DisplayName = "Criar Data Nascimento Zerado Deve Retornar Exception")]
        [Trait("Core", "ValueObjects")]
        public void CriarDataNascimento_ValoresZerados_DeveRetornarException()
        {
            Assert.Throws<DomainException>(() => new DataNascimento(dia: 0, mes: 0, ano: 0));
        }

        [Fact(DisplayName = "Criar Data Nascimento Dia Inválido Deve Retornar Exception")]
        [Trait("Core", "ValueObjects")]
        public void CriarDataNascimento_DiaInvalido_DeveRetornarException()
        {
            Assert.Throws<DomainException>(() => new DataNascimento(dia: 32, mes: 12, ano: 2000));
        }

        [Fact(DisplayName = "Criar Data Nascimento Mês Inválido Deve Retornar Exception")]
        [Trait("Core", "ValueObjects")]
        public void CriarDataNascimento_MesInvalido_DeveRetornarException()
        {
            Assert.Throws<DomainException>(() => new DataNascimento(dia: 1, mes: 13, ano: 2000));
        }

        [Fact(DisplayName = "Criar Data Nascimento Maior Que Data Atual Deve Retornar Exception")]
        [Trait("Core", "ValueObjects")]
        public void CriarDataNascimento_MenorQueDataAtual_DeveRetornarException()
        {
            Assert.Throws<DomainException>(() => new DataNascimento(dia: 16, mes: 4, ano: 2030));
        }

        [Fact(DisplayName = "Criar Data Nascimento Data Inválida Deve Retornar Exception")]
        [Trait("Core", "ValueObjects")]
        public void CriarDataNascimento_DataInvalida_DeveRetornarException()
        {
            Assert.Throws<DomainException>(() => new DataNascimento(dia: 30, mes: 2, ano: 2026));
        }

        [Fact(DisplayName = "Criar Data Nascimento Data Válida Deve Retornar Nascimento")]
        [Trait("Core", "ValueObjects")]
        public void CriarDataNascimento_DataValida_DeveRetornarNascimento()
        {
            var dia = 15;
            var mes = 3;
            var ano = 1998;

            var dataNascimento = new DataNascimento(dia, mes, ano);

            Assert.Equal(dia, dataNascimento.Nascimento.Day);
            Assert.Equal(mes, dataNascimento.Nascimento.Month);
            Assert.Equal(ano, dataNascimento.Nascimento.Year);
        }

        [Fact(DisplayName = "Criar Data Nascimento Válida Com DateTime Deve Retornar Nascimento")]
        [Trait("Core", "ValueObjects")]
        public void CriarDataNascimentoComDateTime_DataValida_DeveRetornarNascimento()
        {
            var dia = 15;
            var mes = 3;
            var ano = 1998;
            var nasc = new DateTime(ano, mes, dia);

            var dataNascimento = new DataNascimento(nasc);

            Assert.Equal(dia, dataNascimento.Nascimento.Day);
            Assert.Equal(mes, dataNascimento.Nascimento.Month);
            Assert.Equal(ano, dataNascimento.Nascimento.Year);
        }
    }
}
