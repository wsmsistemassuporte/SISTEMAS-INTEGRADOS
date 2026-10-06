using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace WSM.SISTEMAS.Core.Usuarios
{
    public class AspNetUsers : IAspNetUser
    {
        private readonly IHttpContextAccessor _accessor;

        public AspNetUsers(IHttpContextAccessor accessor)
        {
            _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        }

        public string Name => _accessor.HttpContext?.User?.Identity?.Name
            ?? _accessor.HttpContext?.User?.GetUserEmail()
            ?? string.Empty;

        public Guid ObterUserId()
        {
            if (!EstaAutenticado()) return Guid.Empty;

            var claimId = _accessor.HttpContext?.User?.GetUserId();
            return Guid.TryParse(claimId, out var userId) ? userId : Guid.Empty;
        }

        public string ObterUserEmail()
        {
            return EstaAutenticado()
                ? _accessor.HttpContext?.User?.GetUserEmail() ?? string.Empty
                : string.Empty;
        }

        public string ObterUserToken()
        {
            if (!EstaAutenticado()) return string.Empty;

            var tokenClaim = _accessor.HttpContext?.User?.GetUserToken();
            if (!string.IsNullOrEmpty(tokenClaim))
            {
                return tokenClaim;
            }

            var authorizationHeader = _accessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authorizationHeader.Substring("Bearer ".Length).Trim();
            }

            return string.Empty;
        }

        public string ObterUserRefreshToken()
        {
            if (!EstaAutenticado()) return string.Empty;

            var refreshTokenClaim = _accessor.HttpContext?.User?.GetUserRefreshToken();
            if (!string.IsNullOrEmpty(refreshTokenClaim))
            {
                return refreshTokenClaim;
            }

            var headerValue = _accessor.HttpContext?.Request.Headers["RefreshToken"].ToString();
            return headerValue ?? string.Empty;
        }

        public bool EstaAutenticado()
        {
            return _accessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }

        public bool PossuiRole(string role)
        {
            return _accessor.HttpContext?.User?.IsInRole(role) ?? false;
        }

        public IEnumerable<Claim> ObterClaims()
        {
            return _accessor.HttpContext?.User?.Claims ?? Enumerable.Empty<Claim>();
        }

        public HttpContext ObterHttpContext()
        {
            return _accessor.HttpContext!;
        }
    }
}
