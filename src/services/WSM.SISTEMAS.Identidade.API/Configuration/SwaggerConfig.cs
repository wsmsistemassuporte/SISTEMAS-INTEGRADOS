using Microsoft.OpenApi.Models;

namespace WSM.SISTEMAS.Identidade.API.Configuration
{
    /// <summary>
    /// Define a documentação interativa da API e o suporte a autenticação JWT no Swagger.
    /// </summary>
    public static class SwaggerConfig
    {
        /// <summary>
        /// Registra os serviços que geram o contrato OpenAPI e a UI de documentação.
        /// </summary>
        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
        {
            // Habilita descoberta automática dos endpoints da aplicação.
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                // Metadados exibidos na página de documentação da API.
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "WSM Sistemas Identidade API",
                    Version = "v1",
                    Description = "API de identidade, autenticacao e emissao de tokens JWT."
                });

                // Declara o padrão de autenticação Bearer para teste de endpoints protegidos.
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Informe o token JWT no formato: Bearer {token}",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };

                // Disponibiliza o esquema de segurança e aplica como requisito global.
                options.AddSecurityDefinition("Bearer", securityScheme);
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    [securityScheme] = Array.Empty<string>()
                });
            });

            return services;
        }

        /// <summary>
        /// Publica a documentação da API no ambiente de desenvolvimento.
        /// </summary>
        public static WebApplication UseSwaggerConfiguration(this WebApplication app)
        {
            // Mantém a superfície de documentação ativa apenas em desenvolvimento.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    // Define o endpoint do contrato e o nome exibido no seletor da UI.
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "WSM Sistemas Identidade API v1");
                });
            }

            return app;
        }
    }
}
