using wrench.auto.repair.core.DomainObjects;
using wrench.auto.repair.core.ValueObjects;

namespace wrench.auto.repair.autenticacao.domain.Entities
{
    public class Usuario : Entity, IAggregateRoot
    {
        /// <summary>Construtor sem parametros exigido pelo Entity Framework Core para materializar o tipo a partir do banco.</summary>
        private Usuario() { }

        public Usuario(Email email, Guid perfilId, bool ativo, DateTime dataCadastro)
        {
            Email = email;
            PerfilId = perfilId;
            Ativo = ativo;
            DataCadastro = dataCadastro;

            Validar();
        }

        public Email Email { get; private set; }
        public string? Senha { get; private set; }
        public Guid PerfilId { get; private set; }
        public bool Ativo { get; private set; }
        public DateTime DataCadastro { get; private set; }
        public Perfil Perfil { get; private set; }

        private void Validar()
        {
            Validacoes.ValidarSeNulo(Email, "E-mail deve ser informado");
            Validacoes.ValidarSeVerdadeiro(PerfilId == Guid.Empty, "Perfil não pode ser vazio");
        }

        public void DefinirSenha(string hashSenha)
        {
            Validacoes.ValidarSeVazio(hashSenha, "Senha não pode ser vazio");
            Validacoes.ValidarSeDiferente(hashSenha.Length, 60, "O hash da senha deve ter 60 caracteres");

            Senha = hashSenha;
        }

        public void AlterarPerfil(Perfil perfil)
        {
            Perfil = perfil;
            PerfilId = perfil.Id;
        }

        public void Inativar()
        {
            Ativo = false;
        }

        public void Ativar()
        {
            Ativo = true;
        }

        public void ResetarSenha()
        {
            Senha = null;
        }
    }
}
