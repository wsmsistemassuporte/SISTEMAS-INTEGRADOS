using Microsoft.AspNetCore.Http;

namespace WSM.SISTEMAS.WebAPI.Core.Exceptions
{
    public class ApiException : Exception
    {
        public ApiException(
            string message,
            int statusCode = StatusCodes.Status400BadRequest,
            string? title = null,
            string? type = null,
            Exception? innerException = null) : base(message, innerException)
        {
            if (statusCode is < 400 or > 599)
            {
                throw new ArgumentOutOfRangeException(nameof(statusCode), "O status code deve estar entre 400 e 599.");
            }

            StatusCode = statusCode;
            Title = title ?? GetDefaultTitle(statusCode);
            Type = type ?? $"https://httpstatuses.com/{statusCode}";
        }

        public int StatusCode { get; }

        public string Title { get; }

        public string Type { get; }

        private static string GetDefaultTitle(int statusCode) =>
            statusCode switch
            {
                StatusCodes.Status400BadRequest => "Requisicao invalida.",
                StatusCodes.Status401Unauthorized => "Nao autenticado.",
                StatusCodes.Status403Forbidden => "Acesso negado.",
                StatusCodes.Status404NotFound => "Recurso nao encontrado.",
                StatusCodes.Status409Conflict => "Conflito ao processar a requisicao.",
                StatusCodes.Status422UnprocessableEntity => "Requisicao nao processavel.",
                _ when statusCode >= 500 => "Erro interno no servidor.",
                _ => "Erro ao processar a requisicao."
            };
    }
}
