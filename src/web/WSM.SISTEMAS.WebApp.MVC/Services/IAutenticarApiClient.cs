using WSM.SISTEMAS.WebApp.MVC.Models.Auth;

namespace WSM.SISTEMAS.WebApp.MVC.Services
{
    public interface IAutenticarApiClient
    {
        /// <summary>POST /api/auth/login - autentica o usuario e retorna JWT + refresh token.</summary>
        Task<AuthResult> LoginAsync(string email, string password, CancellationToken ct = default);

        /// <summary>POST /api/auth/register - registra um novo usuario e retorna JWT + refresh token.</summary>
        Task<AuthResult> RegisterAsync(string email, string password, string confirmPassword, string nome, CancellationToken ct = default);

        /// <summary>POST /api/auth/refresh-token - rotaciona o refresh token e retorna novo JWT.</summary>
        Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken ct = default);

        /// <summary>POST /api/auth/revoke-token - revoga o refresh token no servidor.</summary>
        Task LogoutAsync(string accessToken, string refreshToken, CancellationToken ct = default);
    }
}
