using WSM.SISTEMAS.Core.Usuarios;

namespace WSM.SISTEMAS.WebApp.MVC.Configuration
{
    public static class DependencyInjectionConfig
    {
        public static WebApplicationBuilder AddDependencyInjectionConfiguration(this WebApplicationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IAspNetUser, AspNetUsers>();

            return builder;
        }
    }
}
