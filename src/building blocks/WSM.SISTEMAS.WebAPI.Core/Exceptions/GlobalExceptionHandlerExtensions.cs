using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace WSM.SISTEMAS.WebAPI.Core.Exceptions
{
    public static class GlobalExceptionHandlerExtensions
    {
        public static IServiceCollection AddGlobalExceptionHandler(this IServiceCollection services)
        {
            services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Extensions.TryAdd(
                        "traceId",
                        context.HttpContext.TraceIdentifier);
                };
            });

            services.AddExceptionHandler<GlobalExceptionHandler>();

            return services;
        }

        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler();

            return app;
        }
    }
}
