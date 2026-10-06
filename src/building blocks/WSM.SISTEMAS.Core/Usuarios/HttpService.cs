using System.Text;
using System.Text.Json;

namespace WSM.SISTEMAS.Core.Usuarios
{
    /// <summary>
    /// Classe base utilitária para serviços e clientes HTTP tipados da solução.
    /// Fornece métodos padronizados para criação de payload JSON, deserialização resiliente
    /// e extração unificada e limpa de mensagens de erro das respostas da API.
    /// </summary>
    public abstract class HttpService
    {
        protected static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        protected static StringContent ObterConteudo(object dado)
        {
            return new StringContent(
                JsonSerializer.Serialize(dado, JsonOptions),
                Encoding.UTF8,
                "application/json");
        }

        protected static async Task<T?> DeserializarObjetoResponse<T>(HttpResponseMessage responseMessage, CancellationToken cancellationToken = default)
        {
            var body = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(body))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(body, JsonOptions);
        }

        protected static async Task<List<string>> ExtrairErrosResponseAsync(HttpResponseMessage responseMessage, CancellationToken cancellationToken = default)
        {
            var body = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(body))
            {
                return ["Ocorreu um erro ao processar a solicitação no servidor."];
            }

            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                var erros = ExtrairErrosDeObjetoValidation(root)
                    .Concat(ExtrairErrosDeArray(root, "Mensagens", "mensagens"))
                    .Concat(ExtrairErrosDePropriedade(root, "title", "detail"))
                    .Where(msg => !string.IsNullOrWhiteSpace(msg))
                    .Distinct()
                    .ToList();

                if (erros.Count > 0)
                {
                    return erros;
                }
            }
            catch (JsonException)
            {
                // Se o corpo não for JSON (ex: resposta HTML de Gateway/Proxy)
            }

            return ["Ocorreu um erro inesperado ao processar a solicitação."];
        }

        protected static bool TratarErrosResponse(HttpResponseMessage responseMessage)
        {
            return responseMessage.IsSuccessStatusCode;
        }

        // ── Métodos Auxiliares Limpos (Sem ifs aninhados) ────────────────────

        private static IEnumerable<string> ExtrairErrosDeObjetoValidation(JsonElement root)
        {
            if (!root.TryGetProperty("errors", out var errorsProp) || errorsProp.ValueKind != JsonValueKind.Object)
                yield break;

            foreach (var prop in errorsProp.EnumerateObject())
            {
                if (prop.Value.ValueKind != JsonValueKind.Array) continue;

                foreach (var item in prop.Value.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String && item.GetString() is { Length: > 0 } str)
                    {
                        yield return str;
                    }
                }
            }
        }

        private static IEnumerable<string> ExtrairErrosDeArray(JsonElement root, params string[] propertyNames)
        {
            foreach (var name in propertyNames)
            {
                if (!root.TryGetProperty(name, out var arrayProp) || arrayProp.ValueKind != JsonValueKind.Array)
                    continue;

                foreach (var item in arrayProp.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String && item.GetString() is { Length: > 0 } str)
                    {
                        yield return str;
                    }
                }
            }
        }

        private static IEnumerable<string> ExtrairErrosDePropriedade(JsonElement root, params string[] propertyNames)
        {
            foreach (var name in propertyNames)
            {
                if (root.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.String && prop.GetString() is { Length: > 0 } str)
                {
                    yield return str;
                }
            }
        }
    }
}
