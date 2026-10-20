using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WSM.SISTEMAS.Identidade.API.Configuration
{ 
    /// <summary>
    /// Define o comportamento de entrada da API: serviços expostos e fluxo de requisição.
    /// </summary>
    public static class ApiConfig
    {
        /// <summary>
        /// Habilita os recursos para a API receber requisições e responder endpoints.
        /// </summary>
        public static IServiceCollection AddApiConfiguration(this IServiceCollection services)
        {
            // Ativa os endpoints HTTP da API.
            services.AddControllers();

            return services;
        }

        /// <summary>
        /// Organiza o pipeline HTTP para segurança, transporte e publicação das rotas.
        /// </summary>
        public static WebApplication UseApiConfiguration(this WebApplication app, IWebHostEnvironment env)
        {
            // Facilita diagnóstico durante desenvolvimento.
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Força comunicação segura.
            app.UseHttpsRedirection();

            // Garante controle de acesso nas requisições.
            app.UseAuthentication();
            app.UseAuthorization();

            // Publica as rotas da API.
            app.MapControllers();

            return app;
        }
    }
}