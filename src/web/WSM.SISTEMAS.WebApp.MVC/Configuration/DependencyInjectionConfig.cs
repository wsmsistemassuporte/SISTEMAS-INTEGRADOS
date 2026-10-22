using WSM.SISTEMAS.Core.Usuarios;
using WSM.SISTEMAS.WebApp.MVC.Services;
using WSM.SISTEMAS.WebApp.MVC.Services.Handlers;

namespace WSM.SISTEMAS.WebApp.MVC.Configuration
{
    public static class DependencyInjectionConfig
    {
        public static WebApplicationBuilder AddDependencyInjectionConfiguration(this WebApplicationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IAspNetUser, AspNetUsers>();
            builder.Services.AddTransient<HttpClientAuthorizationDelegatingHandler>();

            return builder;
        }
    }
}
