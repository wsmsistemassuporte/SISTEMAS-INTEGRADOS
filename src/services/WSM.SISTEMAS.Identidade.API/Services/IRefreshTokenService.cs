namespace WSM.SISTEMAS.Identidade.API.Services
{
    public sealed record RefreshTokenResult(string RawToken, DateTime ExpiresAtUtc);

    public sealed class RefreshTokenValidationResult
    {
        public bool Succeeded { get; private init; }

        public Guid UserId { get; private init; }

        public string? NewRawToken { get; private init; }

        public DateTime NewExpiresAtUtc { get; private init; }

        public static RefreshTokenValidationResult Accepted(Guid userId, string newRawToken, DateTime newExpiresAtUtc) =>
            new() { Succeeded = true, UserId = userId, NewRawToken = newRawToken, NewExpiresAtUtc = newExpiresAtUtc };

        public static RefreshTokenValidationResult Rejected() => new() { Succeeded = false };
    }

    public interface IRefreshTokenService
    {
        Task<RefreshTokenResult> CreateAsync(Guid userId, string? ip, CancellationToken cancellationToken = default);

        Task<RefreshTokenValidationResult> ValidateAndRotateAsync(string rawToken, string? ip, CancellationToken cancellationToken = default);

        Task RevokeAsync(string rawToken, string? ip, CancellationToken cancellationToken = default);

        Task RevokeAllAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
