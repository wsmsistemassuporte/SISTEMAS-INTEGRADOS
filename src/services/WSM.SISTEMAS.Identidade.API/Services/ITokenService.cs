using System.Security.Claims;
using WSM.SISTEMAS.Identidade.API.Models;

namespace WSM.SISTEMAS.Identidade.API.Services
{
    public interface ITokenService
    {
        (string AccessToken, DateTime ExpiresAtUtc) GenerateAccessToken(ApplicationUser user, IList<string> roles, IList<Claim>? userClaims = null);

        string GenerateRefreshToken();
    }
}
