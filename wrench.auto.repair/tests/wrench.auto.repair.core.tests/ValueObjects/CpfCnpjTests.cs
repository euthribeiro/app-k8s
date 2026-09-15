using wrench.auto.repair.core.DomainObjects;
using wrench.auto.repair.core.ValueObjects;

namespace wrench.auto.repair.core.tests.ValueObjects
{
    public class CpfCnpjTests
    {
        [Fact(DisplayName = "Criar Documento Vazio Deve Retornar Exception")]
        [Trait("Core", "ValueObjects")]
        public void CriarCpfCnpj_DocumentoVazio_DeveRetornarException()
        {
            var documento = "";

            Assert.Throws<DomainException>(() => new CpfCnpj(documento));
        }

        [Fact(DisplayName = "Criar Documento Invalido Deve Retornar Exception")]
        [Trait("Core", "ValueObjects")]
        public void CriarCpfCnpj_DocumentoInvalido_DeveRetornarException()
        {
            var documento = "1234";

            Assert.Throws<DomainException>(() => new CpfCnpj(documento));
        }

        [Fact(DisplayName = "Criar Cpf Inválido Deve Retornar Exception")]
        [Trait("Core", "ValueObjects")]
        public void CriarCpfCnpj_CpfInvalido_DeveRetornarException()
        {
            var documentoInvalido = "74580776039";

            Assert.Throws<DomainException>(() => new CpfCnpj(documentoInvalido));
        }

        [Fact(DisplayName = "Criar Cpf Valido Sem Pontuacao Deve Ser Valido")]
        [Trait("Core", "ValueObjects")]
        public void CriarCpfCnpj_CpfValidoSemPontuacao_DeveSerValido()
        {
            var documento = "74580776038";

            var cpf = new CpfCnpj(documento);

            Assert.True(cpf.EhValido());
            Assert.Equal(TipoDocumentoEnum.Cpf, cpf.TipoDocumento);
        }

        [Fact(DisplayName = "Criar Cpf Valido Com Pontuacao Deve Ser Valido")]
        [Trait("Core", "ValueObjects")]
        public void CriarCpfCnpj_CpfValidoComPontuacao_DeveSerValido()
        {
            var documento = "745.807.760-38";

            var cpf = new CpfCnpj(documento);

            Assert.True(cpf.EhValido());
            Assert.Equal(TipoDocumentoEnum.Cpf, cpf.TipoDocumento);
        }

        [Fact(DisplayName = "Criar Cpf Valido Com Pontuacao Deve Retornar Sem Pontuacao")]
        [Trait("Core", "ValueObjects")]
        public void CriarCpfCnpj_CpfValidoComPontuacao_DeveRetornarSemPontuacao()
        {
            var documento = "745.807.760-38";
            var documentoSemPontuacao = "74580776038";

            var cpf = new CpfCnpj(documento);

            Assert.Equal(cpf.Numeracao, documentoSemPontuacao);
            Assert.Equal(TipoDocumentoEnum.Cpf, cpf.TipoDocumento);
        }

        [Fact(DisplayName = "Criar Cpf Valido Sem Pontuacao Deve Retornar Cpf Formatado")]
        [Trait("Core", "ValueObjects")]
        public void CriarCpfCnpj_CpfValidoSemPontuacao_DeveRetornarCpfFormatado()
        {
            var documentoSemPontuacao = "74580776038";
            var documento = "745.807.760-38";

            var cpf = new CpfCnpj(documentoSemPontuacao);

            Assert.Equal(cpf.ObterDocumentoFormatado(), documento);
            Assert.Equal(TipoDocumentoEnum.Cpf, cpf.TipoDocumento);
        }

        [Fact(DisplayName = "Criar Cnpj Inválido Deve Retornar Exception")]
        [Trait("Core", "ValueObjects")]
        public void CriarCpfCnpj_CnpjInvalido_DeveRetornarException()
        {
            var documentoInvalido = "87332913000120";

            Assert.Throws<DomainException>(() => new CpfCnpj(documentoInvalido));
        }

        [Fact(DisplayName = "Criar Cnpj Valido Sem Pontuacao Deve Ser Valido")]
        [Trait("Core", "ValueObjects")]
        public void CriarCpfCnpj_CnpjValidoSemPontuacao_DeveSerValido()
        {
            var documento = "56961187000144";

            var cnpj = new CpfCnpj(documento);

            Assert.True(cnpj.EhValido());
            Assert.Equal(TipoDocumentoEnum.Cnpj, cnpj.TipoDocumento);
        }

        [Fact(DisplayName = "Criar Cnpj Valido Com Pontuacao Deve Ser Valido")]
        [Trait("Core", "ValueObjects")]
        public void CriarCpfCnpj_CnpjValidoComPontuacao_DeveSerValido()
        {
            var documento = "56.961.187/0001-44";

            var cnpj = new CpfCnpj(documento);

            Assert.True(cnpj.EhValido());
            Assert.Equal(TipoDocumentoEnum.Cnpj, cnpj.TipoDocumento);
        }

        [Fact(DisplayName = "Criar Cnpj Valido Com Pontuacao Deve Retornar Sem Pontuacao")]
        [Trait("Core", "ValueObjects")]
        public void CriarCpfCnpj_CnpjValidoComPontuacao_DeveRetornarSemPontuacao()
        {
            var documento = "56.961.187/0001-44";
            var documentoSemPontuacao = "56961187000144";

            var cnpj = new CpfCnpj(documento);

            Assert.Equal(cnpj.Numeracao, documentoSemPontuacao);
            Assert.Equal(TipoDocumentoEnum.Cnpj, cnpj.TipoDocumento);
        }

        [Fact(DisplayName = "Criar Cnpj Valido Sem Pontuacao Deve Retornar Cpf Formatado")]
        [Trait("Core", "ValueObjects")]
        public void CriarCpfCnpj_CnpjValidoSemPontuacao_DeveRetornarCpfFormatado()
        {
            var documentoSemPontuacao = "56961187000144";
            var documento = "56.961.187/0001-44";

            var cnpj = new CpfCnpj(documentoSemPontuacao);

            Assert.Equal(cnpj.ObterDocumentoFormatado(), documento);
            Assert.Equal(TipoDocumentoEnum.Cnpj, cnpj.TipoDocumento);
        }
    }
}
