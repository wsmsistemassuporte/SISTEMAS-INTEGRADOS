using Microsoft.AspNetCore.DataProtection;
using WSM.SISTEMAS.WebAPI.Core.Logging;

namespace WSM.SISTEMAS.Identidade.API.Configuration
{
    /// <summary>
    /// Prepara a base técnica necessária para a API iniciar de forma segura e observável.
    /// </summary>
    public static class InfrastructureConfig
    {
        /// <summary>
        /// Define de onde a aplicação lê configurações, como registra falhas e como protege dados sensíveis em desenvolvimento.
        /// </summary>
        public static WebApplicationBuilder AddInfrastructureConfiguration(
            this WebApplicationBuilder builder,
            string applicationName)
        {
            // Carrega as configurações principais da aplicação.
            builder.Configuration
                .SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            // Garante rastreabilidade de erros por serviço.
            builder.AddErrorFileLogging(applicationName);

            // Permite consumir segredos externos sem expor valores no código.
            builder.Configuration.AddKeyPerFile("/run/secrets", optional: true);

            // Mantém chaves entre execuções locais para não invalidar dados protegidos.
            if (builder.Environment.IsDevelopment())
            {
                var keysPath = Path.Combine(Path.GetTempPath(), applicationName, "DataProtectionKeys");
                Directory.CreateDirectory(keysPath);
                var dataProtection = builder.Services
                    .AddDataProtection()
                    .PersistKeysToFileSystem(new DirectoryInfo(keysPath));

                // No Windows, adiciona proteção nativa às chaves persistidas.
                if (OperatingSystem.IsWindows())
                {
                    dataProtection.ProtectKeysWithDpapi();
                }
            }

            return builder;
        }
    }
}
