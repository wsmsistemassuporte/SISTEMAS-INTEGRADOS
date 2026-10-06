using System.Security.Claims;

namespace WSM.SISTEMAS.Core.Usuarios
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserId(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            var claim = principal.FindFirst(ClaimTypes.NameIdentifier) ?? principal.FindFirst("sub");
            return claim?.Value ?? string.Empty;
        }

        public static string GetUserEmail(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            var claim = principal.FindFirst(ClaimTypes.Email) ?? principal.FindFirst("email");
            return claim?.Value ?? string.Empty;
        }

        public static string GetUserToken(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            var claim = principal.FindFirst("token") ?? principal.FindFirst(ClaimTypes.Authentication);
            return claim?.Value ?? string.Empty;
        }

        public static string GetUserRefreshToken(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            var claim = principal.FindFirst("refreshToken") ?? principal.FindFirst("refresh_token");
            return claim?.Value ?? string.Empty;
        }
    }
}
