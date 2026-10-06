using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WSM.SISTEMAS.Identidade.API.Models;

namespace WSM.SISTEMAS.Identidade.API.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private const string LoginProvider = "wsm";
        private const string TokenName = "refresh_token";

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly JwtSettings _settings;
        private readonly ILogger<RefreshTokenService> _logger;

        public RefreshTokenService(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            IOptions<JwtSettings> options,
            ILogger<RefreshTokenService> logger)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<RefreshTokenResult> CreateAsync(Guid userId, string? ip, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                throw new InvalidOperationException($"Usuario {userId} nao encontrado.");
            }

            var rawToken = _tokenService.GenerateRefreshToken();
            var expiresAtUtc = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpirationDays);
            var value = FormatValue(Hash(rawToken), expiresAtUtc);

            await _userManager.RemoveAuthenticationTokenAsync(user, LoginProvider, TokenName);
            var result = await _userManager.SetAuthenticationTokenAsync(user, LoginProvider, TokenName, value);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Falha ao armazenar refresh token: {errors}");
            }

            _logger.LogDebug("Refresh token emitido para o usuario {UserId} a partir de {Ip}.", userId, ip);

            return new RefreshTokenResult(rawToken, expiresAtUtc);
        }

        public async Task<RefreshTokenValidationResult> ValidateAndRotateAsync(string rawToken, string? ip, CancellationToken cancellationToken = default)
        {
            var hash = Hash(rawToken);
            var user = await FindUserByTokenHashAsync(hash, cancellationToken);

            if (user is null)
            {
                _logger.LogWarning("Refresh token desconhecido apresentado a partir de {Ip}.", ip);
                return RefreshTokenValidationResult.Rejected();
            }

            var storedValue = await _userManager.GetAuthenticationTokenAsync(user, LoginProvider, TokenName);
            if (string.IsNullOrWhiteSpace(storedValue) || !TryParseValue(storedValue, out var storedHash, out var expiresAtUtc))
            {
                _logger.LogWarning("Refresh token do usuario {UserId} sem valor armazenado valido.", user.Id);
                return RefreshTokenValidationResult.Rejected();
            }

            if (!string.Equals(storedHash, hash, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Refresh token do usuario {UserId} nao confere com o valor armazenado.", user.Id);
                return RefreshTokenValidationResult.Rejected();
            }

            if (expiresAtUtc <= DateTime.UtcNow)
            {
                _logger.LogInformation("Refresh token expirado para o usuario {UserId}.", user.Id);
                await _userManager.RemoveAuthenticationTokenAsync(user, LoginProvider, TokenName);
                return RefreshTokenValidationResult.Rejected();
            }

            var newRawToken = _tokenService.GenerateRefreshToken();
            var newExpiresAtUtc = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpirationDays);
            var newValue = FormatValue(Hash(newRawToken), newExpiresAtUtc);

            await _userManager.SetAuthenticationTokenAsync(user, LoginProvider, TokenName, newValue);

            _logger.LogInformation("Refresh token rotacionado para o usuario {UserId} a partir de {Ip}.", user.Id, ip);

            return RefreshTokenValidationResult.Accepted(user.Id, newRawToken, newExpiresAtUtc);
        }

        public async Task RevokeAsync(string rawToken, string? ip, CancellationToken cancellationToken = default)
        {
            var hash = Hash(rawToken);
            var user = await FindUserByTokenHashAsync(hash, cancellationToken);

            if (user is null)
            {
                return;
            }

            await _userManager.RemoveAuthenticationTokenAsync(user, LoginProvider, TokenName);
            _logger.LogInformation("Refresh token do usuario {UserId} revogado a partir de {Ip}.", user.Id, ip);
        }

        public async Task RevokeAllAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                return;
            }

            await _userManager.RemoveAuthenticationTokenAsync(user, LoginProvider, TokenName);
            _logger.LogInformation("Refresh tokens do usuario {UserId} revogados.", userId);
        }

        private async Task<ApplicationUser?> FindUserByTokenHashAsync(string hash, CancellationToken cancellationToken)
        {
            var users = await _userManager.Users.ToListAsync(cancellationToken);

            foreach (var user in users)
            {
                var storedValue = await _userManager.GetAuthenticationTokenAsync(user, LoginProvider, TokenName);
                if (!string.IsNullOrWhiteSpace(storedValue) && TryParseValue(storedValue, out var storedHash, out _))
                {
                    if (string.Equals(storedHash, hash, StringComparison.OrdinalIgnoreCase))
                    {
                        return user;
                    }
                }
            }

            return null;
        }

        private static string FormatValue(string hash, DateTime expiresAtUtc) =>
            $"{hash}|{expiresAtUtc:O}";

        private static bool TryParseValue(string value, out string hash, out DateTime expiresAtUtc)
        {
            hash = string.Empty;
            expiresAtUtc = DateTime.MinValue;

            var parts = value.Split('|');
            if (parts.Length < 2 || !DateTime.TryParse(parts[1], out expiresAtUtc))
            {
                return false;
            }

            hash = parts[0];
            return true;
        }

        private static string Hash(string rawToken)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
            return Convert.ToHexString(bytes);
        }
    }
}
