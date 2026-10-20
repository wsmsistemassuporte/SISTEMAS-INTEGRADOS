using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WSM.SISTEMAS.Identidade.API.Data;
using WSM.SISTEMAS.Identidade.API.Extensions;
using WSM.SISTEMAS.Identidade.API.Models;
using WSM.SISTEMAS.Identidade.API.Services;

namespace WSM.SISTEMAS.Identidade.API.Configuration
{
    /// <summary>
    /// Centraliza a configuração de identidade, autenticação JWT e autorização da API.
    /// </summary>
    public static class IdentityConfig
    {
        /// <summary>
        /// Registra persistência do Identity, políticas de senha e validação de token JWT.
        /// </summary>
        public static WebApplicationBuilder AddIdentityConfiguration(this WebApplicationBuilder builder)
        {
            // Conecta o Identity ao banco configurado da aplicação.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:DefaultConnection nao foi configurada.");
            }

            builder.Services.AddDbContext<AutenticarDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services
                .AddIdentity<ApplicationUser, ApplicationRole>(options =>
                {
                    // Define política de senha e regras básicas de usuário.
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = false;

                    options.User.RequireUniqueEmail = true;
                    options.SignIn.RequireConfirmedEmail = false;
                })
                .AddErrorDescriber<IdentityMensagensPortugues>()
                .AddEntityFrameworkStores<AutenticarDbContext>()
                .AddDefaultTokenProviders();

            // Lê e registra as opções de JWT vindas da configuração.
            var jwtSection = builder.Configuration.GetSection(nameof(JwtSettings));
            builder.Services.Configure<JwtSettings>(jwtSection);

            var jwtSettings = jwtSection.Get<JwtSettings>()
                ?? throw new InvalidOperationException("JwtSettings nao foi configurado.");

            if (string.IsNullOrWhiteSpace(jwtSettings.SigningKey))
            {
                throw new InvalidOperationException("JwtSettings:SigningKey nao foi configurado.");
            }

            if (string.IsNullOrWhiteSpace(jwtSettings.Issuer))
            {
                throw new InvalidOperationException("JwtSettings:Issuer nao foi configurado.");
            }

            if (string.IsNullOrWhiteSpace(jwtSettings.Audience))
            {
                throw new InvalidOperationException("JwtSettings:Audience nao foi configurado.");
            }

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey));

            // Configura autenticação via Bearer Token com validações de segurança.
            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = signingKey,
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(2)
                    };
                });

            // Habilita autorização e registra serviços de emissão/renovação de token.
            builder.Services.AddAuthorization();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

            return builder;
        }
    }
}
