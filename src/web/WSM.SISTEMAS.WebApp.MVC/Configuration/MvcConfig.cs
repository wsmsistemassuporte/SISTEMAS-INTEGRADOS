using Microsoft.AspNetCore.Mvc.DataAnnotations;
using WSM.SISTEMAS.WebApp.MVC.Extensions;

namespace WSM.SISTEMAS.WebApp.MVC.Configuration
{
    public static class MvcConfig
    {
        public static WebApplicationBuilder AddMvcConfiguration(this WebApplicationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Services.AddControllersWithViews();
            builder.Services.AddSingleton<IValidationAttributeAdapterProvider, CpfValidationAttributeAdapterProvider>();

            return builder;
        }
    }
}
