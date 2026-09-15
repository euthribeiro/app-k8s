using Microsoft.Extensions.Logging;
using MapperProfiles = wrench.auto.repair.autenticacao.application.AutoMapper;
using wrench.auto.repair.autenticacao.application.Queries.ViewModels;
using wrench.auto.repair.autenticacao.domain.Entities;
using wrench.auto.repair.core.ValueObjects;

namespace wrench.auto.repair.autenticacao.application.tests.AutoMapper
{
    public class UsuarioPerfilProfileTests
    {
        [Fact(DisplayName = "Perfis AutoMapper devem estar configurados e mapear entidades")]
        [Trait("Autenticacao", "Application")]
        public void UsuarioEPerfilProfiles_DeveMapearParaViewModels()
        {
            var loggerFactory = LoggerFactory.Create(_ => { });
            var config = new global::AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfiles.UsuarioProfile>();
                cfg.AddProfile<MapperProfiles.PerfilProfile>();
            }, loggerFactory);
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();

            var perfil = new Perfil("Admin", "Administrador", true, DateTime.UtcNow);
            var usuario = new Usuario(new Email("admin@test.com"), perfil.Id, true, DateTime.UtcNow);
            usuario.AlterarPerfil(perfil);

            var usuarioVm = mapper.Map<UsuarioViewModel>(usuario);
            Assert.Equal("admin@test.com", usuarioVm.Email);
            Assert.NotNull(usuarioVm.Perfil);
            Assert.Equal("Admin", usuarioVm.Perfil.Nome);

            var perfilVm = mapper.Map<PerfilViewModel>(perfil);
            Assert.Equal(perfil.Id, perfilVm.Id);
            Assert.Equal("Admin", perfilVm.Nome);
        }
    }
}
