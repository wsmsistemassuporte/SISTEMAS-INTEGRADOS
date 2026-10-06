using Microsoft.Extensions.Options;
using WSM.SISTEMAS.WebApp.MVC.Services;

namespace WSM.SISTEMAS.WebApp.MVC.Configuration
{
    public static class HttpClientConfig
    {
        public static WebApplicationBuilder AddHttpClientConfiguration(this WebApplicationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Services.Configure<ApiSettings>(
                builder.Configuration.GetSection(nameof(ApiSettings)));

            builder.Services.AddHttpClient<IAutenticarApiClient, AutenticarApiClient>((serviceProvider, client) =>
            {
                var settings = serviceProvider.GetRequiredService<IOptions<ApiSettings>>().Value;
                if (string.IsNullOrWhiteSpace(settings.IdentidadeUrl))
                {
                    throw new InvalidOperationException("ApiSettings:IdentidadeUrl nao foi configurado.");
                }

                client.BaseAddress = new Uri(settings.IdentidadeUrl);
            });

            return builder;
        }
    }
}
