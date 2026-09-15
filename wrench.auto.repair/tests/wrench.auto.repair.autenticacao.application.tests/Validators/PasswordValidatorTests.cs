using FluentValidation;
using wrench.auto.repair.autenticacao.application.Validators;

namespace wrench.auto.repair.autenticacao.application.tests.Validators
{
    [Trait("Autenticacao", "Application")]
    public class PasswordValidatorTests
    {
        private static FluentValidation.Results.ValidationResult Validate(
            string password,
            string phone = "",
            string? idNumber = null,
            DateTime? dob = null)
        {
            var validator = new PasswordValidator(phone, idNumber, dob);
            return validator.Validate(new ValidationContext<string>(password));
        }

        [Fact(DisplayName = "Senha forte com 13+ caracteres deve ser válida")]
        public void SenhaForte_DevePassar()
        {
            var result = Validate("Zx9#Qw3@Mn5$kP!");

            Assert.True(result.IsValid);
        }

        [Fact(DisplayName = "Senha com 12 caracteres e três tipos de caracteres deve ser válida")]
        public void SenhaCom12CaracteresE_TresTipos_DevePassar()
        {
            var result = Validate("Xm9#Zp2$Kq7@");

            Assert.True(result.IsValid);
        }

        [Fact(DisplayName = "Senha com 12 caracteres e apenas dois tipos deve falhar no comprimento")]
        public void SenhaCom12CaracteresE_ApenasDoisTipos_DeveFalhar()
        {
            var result = Validate("ACEGacegaceg");

            Assert.False(result.IsValid);
        }

        [Fact(DisplayName = "Senha com menos de 12 caracteres deve falhar")]
        public void SenhaCurta_DeveFalhar()
        {
            var result = Validate("Aa1!Aa1!");

            Assert.False(result.IsValid);
        }

        [Fact(DisplayName = "Senha sem letra maiúscula deve falhar")]
        public void SenhaSemMaiuscula_DeveFalhar()
        {
            var result = Validate("zx9#qw3@mn5$kp!");

            Assert.False(result.IsValid);
        }

        [Fact(DisplayName = "Senha com quatro caracteres idênticos consecutivos deve falhar")]
        public void SenhaComQuatroIdenticosConsecutivos_DeveFalhar()
        {
            var result = Validate("Xk9#mP2$vL8@nQQQQ!");

            Assert.False(result.IsValid);
        }

        [Fact(DisplayName = "Senha com sequência de teclado deve falhar")]
        public void SenhaComSequenciaDeTeclado_DeveFalhar()
        {
            var result = Validate("Zx9#Qw3@asdfMn5$kP!");

            Assert.False(result.IsValid);
        }

        [Fact(DisplayName = "Senha com sequência alfabética deve falhar")]
        public void SenhaComSequenciaAlfabetica_DeveFalhar()
        {
            var result = Validate("Zx9#Qw3@MabcdEf5$kP!");

            Assert.False(result.IsValid);
        }

        [Fact(DisplayName = "Senha não pode conter trecho do telefone")]
        public void SenhaComTrechoTelefone_DeveFalhar()
        {
            var result = Validate("Zx9#Qw998877@Mn5$kP!", phone: "119988776655");

            Assert.False(result.IsValid);
        }

        [Fact(DisplayName = "Senha não pode conter trecho do documento")]
        public void SenhaComTrechoDocumento_DeveFalhar()
        {
            var result = Validate("Zx9#Qw7890@Mn5$kP!", idNumber: "12345678901234");

            Assert.False(result.IsValid);
        }

        [Fact(DisplayName = "Senha não pode conter trecho da data de nascimento")]
        public void SenhaComTrechoDataNascimento_DeveFalhar()
        {
            var dob = new DateTime(1990, 5, 3);
            var result = Validate("Zx9#Qw1990@Mn5$kP!", dob: dob);

            Assert.False(result.IsValid);
        }
    }
}
