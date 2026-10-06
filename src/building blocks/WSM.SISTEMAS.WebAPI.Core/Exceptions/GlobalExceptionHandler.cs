using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WSM.SISTEMAS.WebAPI.Core.Exceptions
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly IHostEnvironment _environment;
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IProblemDetailsService _problemDetailsService;

        public GlobalExceptionHandler(
            IHostEnvironment environment,
            ILogger<GlobalExceptionHandler> logger,
            IProblemDetailsService problemDetailsService)
        {
            _environment = environment;
            _logger = logger;
            _problemDetailsService = problemDetailsService;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (httpContext.Response.HasStarted)
            {
                _logger.LogWarning(
                    exception,
                    "Nao foi possivel tratar a excecao porque a resposta ja foi iniciada. TraceId: {TraceId}. Path: {Path}. Method: {Method}. ExceptionType: {ExceptionType}",
                    httpContext.TraceIdentifier,
                    httpContext.Request.Path,
                    httpContext.Request.Method,
                    exception.GetType().FullName);

                return false;
            }

            var exceptionDetails = MapException(exception);

            LogException(httpContext, exception, exceptionDetails.Status);

            var problemDetails = new ProblemDetails
            {
                Type = exceptionDetails.Type,
                Title = exceptionDetails.Title,
                Status = exceptionDetails.Status,
                Detail = exceptionDetails.Detail,
                Instance = httpContext.Request.Path
            };

            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

            if (_environment.IsDevelopment())
            {
                problemDetails.Extensions["exceptionType"] = exception.GetType().FullName;
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            }

            httpContext.Response.StatusCode = exceptionDetails.Status;

            var problemDetailsContext = new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problemDetails
            };

            if (await _problemDetailsService.TryWriteAsync(problemDetailsContext))
            {
                return true;
            }

            httpContext.Response.ContentType = "application/problem+json";
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        private ExceptionDetails MapException(Exception exception)
        {
            return exception switch
            {
                ApiException apiException => new ExceptionDetails(
                    apiException.StatusCode,
                    apiException.Title,
                    apiException.Message,
                    apiException.Type),

                BadHttpRequestException badHttpRequestException => new ExceptionDetails(
                    NormalizeStatusCode(badHttpRequestException.StatusCode, StatusCodes.Status400BadRequest),
                    "Requisicao invalida.",
                    badHttpRequestException.Message,
                    $"https://httpstatuses.com/{NormalizeStatusCode(badHttpRequestException.StatusCode, StatusCodes.Status400BadRequest)}"),

                UnauthorizedAccessException => new ExceptionDetails(
                    StatusCodes.Status403Forbidden,
                    "Acesso negado.",
                    exception.Message,
                    "https://httpstatuses.com/403"),

                KeyNotFoundException => new ExceptionDetails(
                    StatusCodes.Status404NotFound,
                    "Recurso nao encontrado.",
                    exception.Message,
                    "https://httpstatuses.com/404"),

                _ => new ExceptionDetails(
                    StatusCodes.Status500InternalServerError,
                    "Erro interno no servidor.",
                    _environment.IsDevelopment() ? exception.Message : "Ocorreu um erro inesperado ao processar a requisicao.",
                    "https://httpstatuses.com/500")
            };
        }

        private void LogException(HttpContext httpContext, Exception exception, int statusCode)
        {
            const string messageTemplate = "Excecao capturada pelo handler global. TraceId: {TraceId}. Path: {Path}. Method: {Method}. StatusCode: {StatusCode}. ExceptionType: {ExceptionType}";

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(
                    exception,
                    messageTemplate,
                    httpContext.TraceIdentifier,
                    httpContext.Request.Path,
                    httpContext.Request.Method,
                    statusCode,
                    exception.GetType().FullName);

                return;
            }

            _logger.LogWarning(
                exception,
                messageTemplate,
                httpContext.TraceIdentifier,
                httpContext.Request.Path,
                httpContext.Request.Method,
                statusCode,
                exception.GetType().FullName);
        }

        private static int NormalizeStatusCode(int statusCode, int fallbackStatusCode) =>
            statusCode is >= 400 and <= 599 ? statusCode : fallbackStatusCode;

        private sealed record ExceptionDetails(int Status, string Title, string Detail, string Type);
    }
}
