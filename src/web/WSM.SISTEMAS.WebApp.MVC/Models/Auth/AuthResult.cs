using WSM.SISTEMAS.WebApp.MVC.Models;

namespace WSM.SISTEMAS.WebApp.MVC.Models.Auth
{
    public class AuthResult : ServiceResult
    {
        public string? AccessToken { get; set; }

        public DateTime ExpiresAtUtc { get; set; }

        public Guid UserId { get; set; }

        public string? Email { get; set; }

        public string? Nome { get; set; }

        public string? FotoPerfilUrl { get; set; }

        public List<string> Roles { get; set; } = [];

        public string? RefreshToken { get; set; }

        public DateTime RefreshTokenExpiresAtUtc { get; set; }

        public static AuthResult Falha(ApiErrorResult apiError)
        {
            return new AuthResult
            {
                Succeeded = false,
                StatusCode = apiError.StatusCode,
                MensagemUsuario = apiError.MensagemUsuario,
                Errors = apiError.Errors,
                TraceId = apiError.TraceId
            };
        }
    }
}
