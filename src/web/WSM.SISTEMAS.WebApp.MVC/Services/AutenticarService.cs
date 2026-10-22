using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using WSM.SISTEMAS.Core.Usuarios;
using WSM.SISTEMAS.WebApp.MVC.Models;
using WSM.SISTEMAS.WebApp.MVC.Models.Auth;

namespace WSM.SISTEMAS.WebApp.MVC.Services
{
    public class AutenticarService : HttpService, IAutenticarService
    {
        private const string RefreshTokenCookieName = "refreshToken";

        private readonly HttpClient _http;
        private readonly ILogger<AutenticarService> _logger;

        public AutenticarService(HttpClient http, ILogger<AutenticarService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<AuthResult> LoginAsync(string email, string password, CancellationToken ct = default)
        {
            return  await PostAuthAsync(
                "/api/auth/login",
                new { email, password },
                "login",
                ct,
                "E-mail ou senha invalidos.");
        }

        public Task<AuthResult> RegisterAsync(
            string email,
            string password,
            string confirmPassword,
            string nome,
            CancellationToken ct = default)
        {
            return PostAuthAsync(
                "/api/auth/register",
                new { nome, email, password, confirmPassword },
                "cadastro de usuario",
                ct);
        }

        public Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken ct = default)
        {
            return PostAuthAsync(
                "/api/auth/refresh-token",
                new { refreshToken },
                "refresh token",
                ct,
                "Sua sessao expirou. Faca login novamente.");
        }

        public async Task LogoutAsync(string accessToken, string refreshToken, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return;
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/revoke-token")
            {
                Content = ObterConteudo(new { refreshToken })
            };

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            try
            {
                var response = await _http.SendAsync(request, ct);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Falha ao revogar refresh token. StatusCode: {StatusCode}",
                        (int)response.StatusCode);
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                _logger.LogWarning(ex, "Nao foi possivel revogar o refresh token durante o logout.");
            }
        }

        private async Task<AuthResult> PostAuthAsync(
            string endpoint,
            object payload,
            string operacao,
            CancellationToken ct,
            string? unauthorizedMessage = null)
        {
            try
            {
                var response = await _http.PostAsync(endpoint, ObterConteudo(payload), ct);
                if (!response.IsSuccessStatusCode)
                {
                    var apiError = await CriarApiErrorResultAsync(response, ct, unauthorizedMessage);

                    _logger.LogWarning(
                        "Falha na operacao de autenticacao {Operacao}. StatusCode: {StatusCode}. TraceId: {TraceId}",
                        operacao,
                        apiError.StatusCode,
                        apiError.TraceId);

                    return AuthResult.Falha(apiError);
                }

                var authResult = await MapToAuthResultAsync(response, ct);
                if (authResult is null)
                {
                    _logger.LogWarning("A API de identidade retornou sucesso sem payload valido na operacao {Operacao}.", operacao);
                    return AuthResult.Falha(CriarErroInterno());
                }

                return authResult;
            }
            catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Timeout ao executar a operacao de autenticacao {Operacao}.", operacao);
                return AuthResult.Falha(CriarErroServicoIndisponivel());
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Falha de comunicacao com a API de identidade na operacao {Operacao}.", operacao);
                return AuthResult.Falha(CriarErroServicoIndisponivel());
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Resposta invalida da API de identidade na operacao {Operacao}.", operacao);
                return AuthResult.Falha(CriarErroInterno());
            }
        }

        private async Task<AuthResult?> MapToAuthResultAsync(HttpResponseMessage response, CancellationToken ct)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(body))
            {
                return null;
            }

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            var refreshTokenFromCookie = ExtractRefreshTokenFromCookie(response);

            return new AuthResult
            {
                Succeeded = true,
                StatusCode = (int)response.StatusCode,
                AccessToken = GetString(root, "accessToken"),
                ExpiresAtUtc = GetDateTime(root, "accessTokenExpiresAtUtc") ?? DateTime.UtcNow,
                UserId = GetGuid(root, "userId") ?? Guid.Empty,
                Email = GetString(root, "email"),
                Nome = GetString(root, "nome"),
                FotoPerfilUrl = GetString(root, "fotoPerfilUrl"),
                Roles = GetStringList(root, "roles"),
                RefreshToken = GetString(root, "refreshToken") ?? refreshTokenFromCookie,
                RefreshTokenExpiresAtUtc = GetDateTime(root, "refreshTokenExpiresAtUtc") ?? DateTime.UtcNow
            };
        }

        private async Task<ApiErrorResult> CriarApiErrorResultAsync(
            HttpResponseMessage response,
            CancellationToken ct,
            string? unauthorizedMessage = null)
        {
            var statusCode = (int)response.StatusCode;
            var mensagemUsuario = unauthorizedMessage ?? ObterMensagemUsuarioPorStatus(response.StatusCode);
            var errors = new List<string>();
            string? traceId = null;

            var body = await response.Content.ReadAsStringAsync(ct);
            if (!string.IsNullOrWhiteSpace(body))
            {
                try
                {
                    using var doc = JsonDocument.Parse(body);
                    var root = doc.RootElement;

                    errors = ExtrairMensagensErro(root).ToList();
                    traceId = GetString(root, "traceId");
                }
                catch (JsonException)
                {
                    _logger.LogDebug("A resposta de erro da API de identidade nao estava em JSON.");
                }
            }

            if (errors.Count == 0)
            {
                errors.Add(mensagemUsuario);
            }

            return new ApiErrorResult
            {
                StatusCode = statusCode,
                MensagemUsuario = mensagemUsuario,
                Errors = errors.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
                TraceId = traceId
            };
        }

        private static ApiErrorResult CriarErroServicoIndisponivel()
        {
            return new ApiErrorResult
            {
                StatusCode = (int)HttpStatusCode.ServiceUnavailable,
                MensagemUsuario = "Servico indisponivel. Tente novamente em instantes.",
                Errors = ["Servico indisponivel. Tente novamente em instantes."]
            };
        }

        private static ApiErrorResult CriarErroInterno()
        {
            return new ApiErrorResult
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                MensagemUsuario = "O servico encontrou um erro. Tente novamente.",
                Errors = ["O servico encontrou um erro. Tente novamente."]
            };
        }

        private static string ObterMensagemUsuarioPorStatus(HttpStatusCode statusCode)
        {
            return statusCode switch
            {
                HttpStatusCode.BadRequest => "Verifique os dados informados.",
                HttpStatusCode.Unauthorized => "E-mail ou senha invalidos.",
                HttpStatusCode.Forbidden => "Voce nao tem permissao para essa acao.",
                HttpStatusCode.NotFound => "Recurso nao encontrado.",
                HttpStatusCode.Conflict => "Ja existe um registro com esses dados.",
                HttpStatusCode.InternalServerError => "O servico encontrou um erro. Tente novamente.",
                HttpStatusCode.ServiceUnavailable => "Servico indisponivel. Tente novamente em instantes.",
                _ => "Nao foi possivel processar sua solicitacao."
            };
        }

        private static IEnumerable<string> ExtrairMensagensErro(JsonElement root)
        {
            var structuredMessages = new List<string>();

            foreach (var message in ExtrairArrayDeStrings(root, "Mensagens"))
            {
                structuredMessages.Add(message);
            }

            foreach (var message in ExtrairArrayDeStrings(root, "mensagens"))
            {
                structuredMessages.Add(message);
            }

            if (TryGetProperty(root, "errors", out var errors))
            {
                foreach (var message in ExtrairErrors(errors))
                {
                    structuredMessages.Add(message);
                }
            }

            if (structuredMessages.Count > 0)
            {
                foreach (var message in structuredMessages)
                {
                    yield return message;
                }

                yield break;
            }

            foreach (var propertyName in new[] { "detail", "title", "message" })
            {
                var message = GetString(root, propertyName);
                if (!string.IsNullOrWhiteSpace(message))
                {
                    yield return message;
                }
            }
        }

        private static IEnumerable<string> ExtrairErrors(JsonElement errors)
        {
            if (errors.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in errors.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(item.GetString()))
                    {
                        yield return item.GetString()!;
                    }
                }

                yield break;
            }

            if (errors.ValueKind != JsonValueKind.Object)
            {
                yield break;
            }

            foreach (var property in errors.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in property.Value.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(item.GetString()))
                        {
                            yield return item.GetString()!;
                        }
                    }
                }
                else if (property.Value.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(property.Value.GetString()))
                {
                    yield return property.Value.GetString()!;
                }
            }
        }

        private static IEnumerable<string> ExtrairArrayDeStrings(JsonElement root, string propertyName)
        {
            if (!TryGetProperty(root, propertyName, out var property) || property.ValueKind != JsonValueKind.Array)
            {
                yield break;
            }

            foreach (var item in property.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(item.GetString()))
                {
                    yield return item.GetString()!;
                }
            }
        }

        private static string? ExtractRefreshTokenFromCookie(HttpResponseMessage response)
        {
            if (!response.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                return null;
            }

            foreach (var cookie in cookies)
            {
                var parts = cookie.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var tokenPart = parts.FirstOrDefault(p => p.StartsWith($"{RefreshTokenCookieName}=", StringComparison.OrdinalIgnoreCase));
                if (tokenPart is null)
                {
                    continue;
                }

                return WebUtility.UrlDecode(tokenPart[(RefreshTokenCookieName.Length + 1)..]);
            }

            return null;
        }

        private static List<string> GetStringList(JsonElement root, string propertyName)
        {
            if (!TryGetProperty(root, propertyName, out var property) || property.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            return property
                .EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => item.GetString())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value!)
                .ToList();
        }

        private static Guid? GetGuid(JsonElement root, string propertyName)
        {
            return TryGetProperty(root, propertyName, out var property) && property.ValueKind == JsonValueKind.String && property.TryGetGuid(out var value)
                ? value
                : null;
        }

        private static DateTime? GetDateTime(JsonElement root, string propertyName)
        {
            return TryGetProperty(root, propertyName, out var property) && property.ValueKind == JsonValueKind.String && property.TryGetDateTime(out var value)
                ? value
                : null;
        }

        private static string? GetString(JsonElement root, string propertyName)
        {
            return TryGetProperty(root, propertyName, out var property) && property.ValueKind == JsonValueKind.String
                ? property.GetString()
                : null;
        }

        private static bool TryGetProperty(JsonElement root, string propertyName, out JsonElement property)
        {
            if (root.ValueKind == JsonValueKind.Object)
            {
                foreach (var item in root.EnumerateObject())
                {
                    if (string.Equals(item.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                    {
                        property = item.Value;
                        return true;
                    }
                }
            }

            property = default;
            return false;
        }
    }
}
