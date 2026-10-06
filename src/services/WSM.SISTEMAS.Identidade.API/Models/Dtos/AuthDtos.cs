using System.ComponentModel.DataAnnotations;

namespace WSM.SISTEMAS.Identidade.API.Models.Dtos
{
    public sealed class RegisterUserRequest
    {
        [Required(ErrorMessage = "O nome e obrigatorio.")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 150 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O email e obrigatorio.")]
        [EmailAddress(ErrorMessage = "O email informado e invalido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha e obrigatoria.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 100 caracteres.")]
        public string Password { get; set; } = string.Empty;

        [Compare(nameof(Password), ErrorMessage = "A confirmacao da senha nao confere.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public sealed class LoginRequest
    {
        [Required(ErrorMessage = "O email e obrigatorio.")]
        [EmailAddress(ErrorMessage = "O email informado e invalido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha e obrigatoria.")]
        public string Password { get; set; } = string.Empty;
    }

    public sealed class RefreshTokenRequest
    {
        [Required(ErrorMessage = "O refresh token e obrigatorio.")]
        public string RefreshToken { get; set; } = string.Empty;
    }

    public sealed class RevokeTokenRequest
    {
        [Required(ErrorMessage = "O refresh token e obrigatorio.")]
        public string RefreshToken { get; set; } = string.Empty;
    }

    public sealed class AuthResponse
    {
        public Guid UserId { get; init; }

        public string Nome { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string AccessToken { get; init; } = string.Empty;

        public DateTime AccessTokenExpiresAtUtc { get; init; }

        public string RefreshToken { get; init; } = string.Empty;

        public DateTime RefreshTokenExpiresAtUtc { get; init; }

        public IEnumerable<string> Roles { get; init; } = [];
    }

    public sealed class UserInfoResponse
    {
        public Guid UserId { get; init; }

        public string Nome { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public IEnumerable<string> Roles { get; init; } = [];
    }

    public sealed class ErrorResponse
    {
        public IEnumerable<string> Errors { get; init; } = [];
    }
}
