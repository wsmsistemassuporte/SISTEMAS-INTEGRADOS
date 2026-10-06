using Microsoft.AspNetCore.Authentication.Cookies;

namespace WSM.SISTEMAS.WebApp.MVC.Configuration
{
    public static class AuthenticationConfig
    {
        public static WebApplicationBuilder AddAuthenticationConfiguration(this WebApplicationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = "WSM.SISTEMAS.WebApp.Auth";
                    options.LoginPath = "/Auth/Login";
                    options.LogoutPath = "/Auth/Logout";
                    options.AccessDeniedPath = "/Auth/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(1);
                    options.SlidingExpiration = true;
                });

            return builder;
        }
    }
}
