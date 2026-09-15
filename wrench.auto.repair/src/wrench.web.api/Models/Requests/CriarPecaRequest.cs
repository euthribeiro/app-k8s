using wrench.auto.repair.estoque.application.Commands;

namespace wrench.web.api.Models.Requests
{
    /// <summary>
    /// Representa a solicitação para criar uma ordem de serviço.
    /// </summary>
    public class CriarPecaRequest
    {
        /// <summary>
        /// Nome da peça
        /// </summary>
        public string Nome { get; set; }

        /// <summary>
        /// Descrição da peça
        /// </summary>
        public string Descricao { get; set; }

        /// <summary>
        /// Valor da peça
        /// </summary>
        public double Valor { get; set; }

        /// <summary>
        /// Quantidade da Peça
        /// </summary>
        public int Quantidade { get; set; }

        /// <summary>
        /// Estado da peça no sistema
        /// </summary>
        public bool Ativo { get; set; }

        public static implicit operator CadastrarPecaCommand(CriarPecaRequest request)
        {
            return new CadastrarPecaCommand(request.Nome, request.Descricao, request.Valor, request.Quantidade, request.Ativo);
        }
    }
}
