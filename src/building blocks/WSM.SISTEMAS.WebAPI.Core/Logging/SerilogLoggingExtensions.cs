using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace WSM.SISTEMAS.WebAPI.Core.Logging
{
    public static class SerilogLoggingExtensions
    {
        public static WebApplicationBuilder AddErrorFileLogging(
            this WebApplicationBuilder builder,
            string applicationName)
        {
            var safeApplicationName = SanitizeFileName(applicationName);
            var logFilePath = Path.Combine("logs", $"{safeApplicationName}-.log");

            builder.Logging.ClearProviders();
            builder.Host.UseSerilog((context, services, loggerConfiguration) =>
            {
                loggerConfiguration
                    .MinimumLevel.Error()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                    .MinimumLevel.Override("System", LogEventLevel.Error)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Application", applicationName)
                    .WriteTo.File(
                        path: logFilePath,
                        restrictedToMinimumLevel: LogEventLevel.Error,
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 31,
                        shared: true,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Application} {TraceId} {SourceContext} {Message:lj}{NewLine}{Exception}");
            });

            return builder;
        }

        public static IApplicationBuilder UseErrorFileRequestLogging(this IApplicationBuilder app)
        {
            app.UseSerilogRequestLogging(options =>
            {
                options.GetLevel = (httpContext, elapsed, exception) =>
                    exception is not null || httpContext.Response.StatusCode >= StatusCodes.Status400BadRequest
                        ? LogEventLevel.Error
                        : LogEventLevel.Verbose;

                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("TraceId", System.Diagnostics.Activity.Current?.Id ?? httpContext.TraceIdentifier);
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? string.Empty);
                    diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                };
            });

            return app;
        }

        private static string SanitizeFileName(string fileName)
        {
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(invalidChar, '-');
            }

            return fileName;
        }
    }
}
