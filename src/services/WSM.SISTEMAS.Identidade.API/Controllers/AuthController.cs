using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WSM.SISTEMAS.Identidade.API.Models;
using WSM.SISTEMAS.Identidade.API.Models.Dtos;
using WSM.SISTEMAS.Identidade.API.Services;
using WSM.SISTEMAS.WebAPI.Core;

namespace WSM.SISTEMAS.Identidade.API.Controllers
{
    [Route("api/auth")]
    public class AuthController : MainController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenService _refreshTokenService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            IRefreshTokenService refreshTokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(RegisterUserRequest request, CancellationToken cancellationToken)
        {
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                Nome = request.Nome,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                AdicionarErrosIdentity(result);
                return CustomResponse();
            }

            var response = await CreateAuthResponseAsync(user, cancellationToken);
            return CustomResponse(StatusCodes.Status201Created, response);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                AdicionarErroProcessamento("Usuario ou senha invalidos.");
                return CustomResponse(StatusCodes.Status401Unauthorized);
            }

            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (!signInResult.Succeeded)
            {
                AdicionarErroProcessamento("Usuario ou senha invalidos.");
                return CustomResponse(StatusCodes.Status401Unauthorized);
            }

            user.UltimoLoginEm = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var response = await CreateAuthResponseAsync(user, cancellationToken);
            return CustomResponse(response);
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var validation = await _refreshTokenService.ValidateAndRotateAsync(request.RefreshToken, GetIpAddress(), cancellationToken);
            if (!validation.Succeeded || validation.NewRawToken is null)
            {
                AdicionarErroProcessamento("Refresh token invalido ou expirado.");
                return CustomResponse(StatusCodes.Status401Unauthorized);
            }

            var user = await _userManager.FindByIdAsync(validation.UserId.ToString());
            if (user is null)
            {
                AdicionarErroProcessamento("Usuario nao encontrado.");
                return CustomResponse(StatusCodes.Status401Unauthorized);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            var accessToken = _tokenService.GenerateAccessToken(user, roles, claims);

            return CustomResponse(new AuthResponse
            {
                UserId = user.Id,
                Nome = user.Nome,
                Email = user.Email ?? string.Empty,
                AccessToken = accessToken.AccessToken,
                AccessTokenExpiresAtUtc = accessToken.ExpiresAtUtc,
                RefreshToken = validation.NewRawToken,
                RefreshTokenExpiresAtUtc = validation.NewExpiresAtUtc,
                Roles = roles
            });
        }

        [HttpPost("revoke-token")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RevokeToken(RevokeTokenRequest request, CancellationToken cancellationToken)
        {
            await _refreshTokenService.RevokeAsync(request.RefreshToken, GetIpAddress(), cancellationToken);
            return CustomResponse(StatusCodes.Status204NoContent);
        }

        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserInfoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                AdicionarErroProcessamento("Usuario nao autenticado.");
                return CustomResponse(StatusCodes.Status401Unauthorized);
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                AdicionarErroProcessamento("Usuario nao encontrado.");
                return CustomResponse(StatusCodes.Status401Unauthorized);
            }

            var roles = await _userManager.GetRolesAsync(user);

            return CustomResponse(new UserInfoResponse
            {
                UserId = user.Id,
                Nome = user.Nome,
                Email = user.Email ?? string.Empty,
                Roles = roles
            });
        }

        private async Task<AuthResponse> CreateAuthResponseAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            var accessToken = _tokenService.GenerateAccessToken(user, roles, claims);
            var refreshToken = await _refreshTokenService.CreateAsync(user.Id, GetIpAddress(), cancellationToken);

            return new AuthResponse
            {
                UserId = user.Id,
                Nome = user.Nome,
                Email = user.Email ?? string.Empty,
                AccessToken = accessToken.AccessToken,
                AccessTokenExpiresAtUtc = accessToken.ExpiresAtUtc,
                RefreshToken = refreshToken.RawToken,
                RefreshTokenExpiresAtUtc = refreshToken.ExpiresAtUtc,
                Roles = roles
            };
        }

        private string? GetIpAddress() =>
            HttpContext.Connection.RemoteIpAddress?.ToString();

        private void AdicionarErrosIdentity(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                AdicionarErroProcessamento(error.Description);
            }
        }
    }
}
