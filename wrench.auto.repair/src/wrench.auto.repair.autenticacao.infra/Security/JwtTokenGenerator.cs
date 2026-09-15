using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using wrench.auto.repair.autenticacao.domain.Entities;
using wrench.auto.repair.autenticacao.domain.Models;
using wrench.auto.repair.autenticacao.domain.Security;

namespace wrench.auto.repair.autenticacao.infra.Security
{
    public class JwtTokenGenerator(IOptions<JwtOptions> _options) : IJwtTokenGenerator
    {
        public TokenAcesso GerarToken(Usuario usuario)
        {
            JwtSecurityTokenHandler tokenHandler = new();
            byte[] key = Encoding.ASCII.GetBytes(_options.Value.Secret);
            SecurityTokenDescriptor tokenDescriptor = DescribeTokenSpecification(usuario, key);
            SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor);
            string token = tokenHandler.WriteToken(securityToken);
            return new TokenAcesso(token, usuario.Email.Endereco, usuario.Perfil.Nome);
        }

        private SecurityTokenDescriptor DescribeTokenSpecification(Usuario usuario, byte[] key)
        {
            return new SecurityTokenDescriptor
            {
                Subject = ConfigureClaimIdentity(usuario),
                Expires = DateTime.UtcNow.AddMinutes(_options.Value.ExpirationMinutes),
                Issuer = _options.Value.Issuer,
                Audience = _options.Value.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
        }

        private ClaimsIdentity ConfigureClaimIdentity(Usuario usuario)
        {
            return new ClaimsIdentity(
            [
                new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new(ClaimTypes.Name, usuario.Email.Endereco.ToString()),
                new(ClaimTypes.Role, usuario.Perfil.Nome.ToString()),
            ]);
        }
    }
}
