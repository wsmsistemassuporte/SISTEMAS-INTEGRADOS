using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using WSM.SISTEMAS.WebApp.MVC.Models.Auth;

namespace WSM.SISTEMAS.WebApp.MVC.Services
{
    /// <summary>
    /// Orquestra a criação e leitura do cookie de autenticação do MVC.
    /// O cookie contém:
    /// - Claims do usuário (sub, email, name, role)
    /// - JWT (access token) como claim ClaimTypes.Authentication
    /// - Refresh token como claim customizada "refreshToken"
    ///
    /// Os nomes batem com o que o Core lê (ClaimsPrincipalExtensions.GetUserToken /
    /// GetUserRefreshToken e IAspNetUser.ObterUserToken/ObterUserRefreshToken), para que
    /// qualquer parte da aplicação obtenha o JWT e encaminhe a APIs downstream.
    /// </summary>
    public static class CookieAuthService
    {
        public const string JwtClaimType = ClaimTypes.Authentication;
        public const string RefreshTokenClaimType = "refreshToken";

        /// <summary>
        /// Cria o cookie de autenticação no MVC a partir do resultado da API.
        /// </summary>
        public static async Task SignInAsync(
            HttpContext httpContext,
            AuthResult authResult,
            bool isPersistent,
            ILogger logger)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, authResult.UserId.ToString()),
                new(ClaimTypes.Email, authResult.Email ?? string.Empty),
                new(ClaimTypes.Name, authResult.Nome ?? authResult.Email ?? string.Empty),

                // JWT e refresh token como claims para acesso facilitado
                new(JwtClaimType, authResult.AccessToken ?? string.Empty),
                new(RefreshTokenClaimType, authResult.RefreshToken ?? string.Empty)
            };

            // Extrai todas as claims presentes no JWT para o Cookie do MVC
            if (!string.IsNullOrEmpty(authResult.AccessToken))
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    if (handler.CanReadToken(authResult.AccessToken))
                    {
                        var jwtToken = handler.ReadJwtToken(authResult.AccessToken);
                        foreach (var claim in jwtToken.Claims)
                        {
                            if (!claims.Any(c => c.Type == claim.Type && c.Value == claim.Value))
                            {
                                claims.Add(new Claim(claim.Type, claim.Value));
                            }
                        }
                    }
                }
                catch
                {
                    // Ignora falha de parse caso o token seja malformado
                }
            }

            foreach (var role in authResult.Roles)
            {
                if (!claims.Any(c => c.Type == ClaimTypes.Role && c.Value == role))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                ExpiresUtc = isPersistent ? authResult.ExpiresAtUtc : null,
                AllowRefresh = true
            };

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties);

            logger.LogInformation(
                "Usuário {UserId} ({Email}) autenticado no MVC via cookie. Expira em {Expires}",
                authResult.UserId, authResult.Email, authResult.ExpiresAtUtc);
        }

        /// <summary>
        /// Remove o cookie de autenticação do MVC.
        /// </summary>
        public static async Task SignOutAsync(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Extrai o JWT (access token) armazenado no cookie de autenticação.
        /// Retorna null se o usuário não estiver autenticado.
        /// </summary>
        public static string? GetJwt(ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(JwtClaimType);
        }

        /// <summary>
        /// Extrai o refresh token armazenado no cookie de autenticação.
        /// Retorna null se o usuário não estiver autenticado.
        /// </summary>
        public static string? GetRefreshToken(ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(RefreshTokenClaimType);
        }
    }
}
