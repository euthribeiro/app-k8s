using wrench.auto.repair.core.DomainObjects;

namespace wrench.auto.repair.core.ValueObjects
{
    public class DataNascimento
    {
        /// <summary>Construtor sem parametros exigido pelo Entity Framework Core para materializar o tipo a partir do banco.</summary>
        protected DataNascimento() { }


        public DateTime Nascimento { get; private set; }

        public DataNascimento(DateTime nascimento)
        {
            Validar(nascimento.Day, nascimento.Month, nascimento.Year);

            Nascimento = nascimento;
        }

        public DataNascimento(int dia, int mes, int ano)
        {
            Validar(dia, mes, ano);

            Nascimento = new DateTime(ano, mes, dia);
        }

        private void Validar(int dia, int mes, int ano)
        {
            Validacoes.ValidarSeMenorQue(dia, 1, "Dia deve ser maior que 0");
            Validacoes.ValidarSeMenorQue(mes, 1, "Mês deve ser maior que 0");
            Validacoes.ValidarSeMenorQue(mes, 1, "Ano deve ser maior que 0");
            Validacoes.ValidarMinimoMaximo(dia, 1, 31, "Dia deve estar entre 1 e 31");
            Validacoes.ValidarMinimoMaximo(mes, 1, 12, "Mês deve estar entre 1 e 12");

            if (!DataValida(dia, mes, ano))
                throw new DomainException("A data informada não é válida.");

            var data = new DateTime(ano, mes, dia);

            if (data > DateTime.UtcNow.Date)
                throw new DomainException("A data não pode ser superior a data atual.");
        }

        private static bool DataValida(int dia, int mes, int ano)
        {
            if (ano < 1 || ano > 9999) return false;
            if (mes < 1 || mes > 12) return false;

            int diasNoMes = DateTime.DaysInMonth(ano, mes);

            return dia >= 1 && dia <= diasNoMes;
        }
    }
}
