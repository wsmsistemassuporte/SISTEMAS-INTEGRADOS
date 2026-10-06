using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WSM.SISTEMAS.Identidade.API.Models;

namespace WSM.SISTEMAS.Identidade.API.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _settings;
        private readonly ILogger<TokenService> _logger;

        public TokenService(IOptions<JwtSettings> options, ILogger<TokenService> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }

        public (string AccessToken, DateTime ExpiresAtUtc) GenerateAccessToken(ApplicationUser user, IList<string> roles, IList<Claim>? userClaims = null)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Nome),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            if (userClaims is { Count: > 0 })
            {
                claims.AddRange(userClaims);
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SigningKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAtUtc = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: expiresAtUtc,
                signingCredentials: credentials);

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            _logger.LogDebug("Access token gerado para o usuario {UserId}, expira em {ExpiresAtUtc}.", user.Id, expiresAtUtc);

            return (accessToken, expiresAtUtc);
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
