using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace wrench.web.api.Configuration
{
    public static class AuthenticationConfiguration
    {
        public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            AuthenticationBuilder authenticationBuilder = services.AddAuthentication(authenticationOptions =>
            {
                authenticationOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authenticationOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            });

            authenticationBuilder.ConfigureJwtBearer(configuration);
        }

        private static void ConfigureJwtBearer(this AuthenticationBuilder authenticationBuilder, IConfiguration configuration)
        {
            var sectionName = "JwtOptions";
            var secret = configuration.GetSection(sectionName).GetValue<string>("Secret");
            var issuer = configuration.GetSection(sectionName).GetValue<string>("Issuer");
            var audience = configuration.GetSection(sectionName).GetValue<string>("Audience");

            if (string.IsNullOrWhiteSpace(secret) ||
                string.IsNullOrWhiteSpace(issuer) ||
                string.IsNullOrWhiteSpace(audience))
                throw new ArgumentNullException(sectionName, "JWT não configurado");

            byte[] key = Encoding.ASCII.GetBytes(secret);

            authenticationBuilder.AddJwtBearer(jwtBearerOptions =>
            {
                jwtBearerOptions.RequireHttpsMetadata = false;
                jwtBearerOptions.SaveToken = true;
                jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    RequireExpirationTime = true,
                    ValidateIssuer = true,
                    ValidateAudience = true
                };
            });
        }
    }
}
