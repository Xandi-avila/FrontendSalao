using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SalaoAdmin.Comum;
using SalaoAdmin.Utilitarios;

namespace SalaoAdmin.Servicos.Api;

public abstract class BaseApiService(
    HttpClient httpClient,
    ErrorHandlerService errorHandler,
    ILogger logger)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ErrorHandlerService _errorHandler = errorHandler;
    private readonly ILogger _logger = logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new ApiNullableDateTimeJsonConverter(),
            new ApiDateTimeJsonConverter()
        }
    };

    protected async Task<ApiResult<TResposta>> GetAsync<TResposta>(string endpoint, CancellationToken cancelamento = default)
    {
        return await ExecutarAsync<TResposta>(
            () => new HttpRequestMessage(HttpMethod.Get, endpoint),
            cancelamento);
    }

    protected async Task<ApiResult<TResposta>> PostAsync<TRequisicao, TResposta>(string endpoint, TRequisicao body, CancellationToken cancelamento = default)
    {
        return await ExecutarAsync<TResposta>(
            () => CriarComJson(HttpMethod.Post, endpoint, body),
            cancelamento);
    }

    protected async Task<ApiResult<TResposta>> PutAsync<TRequisicao, TResposta>(string endpoint, TRequisicao body, CancellationToken cancelamento = default)
    {
        return await ExecutarAsync<TResposta>(
            () => CriarComJson(HttpMethod.Put, endpoint, body),
            cancelamento);
    }

    protected async Task<ApiResult<TResposta>> PatchAsync<TResposta>(string endpoint, CancellationToken cancelamento = default)
    {
        return await ExecutarAsync<TResposta>(
            () => new HttpRequestMessage(HttpMethod.Patch, endpoint),
            cancelamento);
    }

    protected async Task<ApiResult<TResposta>> DeleteAsync<TResposta>(string endpoint, CancellationToken cancelamento = default)
    {
        return await ExecutarAsync<TResposta>(
            () => new HttpRequestMessage(HttpMethod.Delete, endpoint),
            cancelamento);
    }

    private async Task<ApiResult<TResposta>> ExecutarAsync<TResposta>(
        Func<HttpRequestMessage> requestFactory,
        CancellationToken cancelamento = default)
    {
        const int maxTentativas = 2;

        for (var tentativa = 1; tentativa <= maxTentativas; tentativa++)
        {
            try
            {
                using var request = requestFactory();
                using var response = await _httpClient.SendAsync(request, cancelamento);

                if (!response.IsSuccessStatusCode)
                    return await _errorHandler.TratarFalhaHttpAsync<TResposta>(response, cancelamento);

                var envelope = await response.Content.ReadFromJsonAsync<ResultadoApiEnvelope<TResposta>>(cancellationToken: cancelamento);
                if (envelope is null)
                    return ApiResult<TResposta>.Falha("Resposta inválida da API.");

                if (!envelope.Sucesso)
                    return ApiResult<TResposta>.Falha(envelope.Mensagem ?? "Operação falhou.", envelope.Erros);

                return ApiResult<TResposta>.Ok(envelope.Dados, envelope.Mensagem);
            }
            catch (Exception ex) when (tentativa < maxTentativas && EhErroRecuperavel(ex))
            {
                _logger.LogWarning(ex, "Tentativa {Tentativa} falhou, repetindo chamada.", tentativa);
                await Task.Delay(300 * tentativa, cancelamento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado de integração com API.");
                return _errorHandler.TratarExcecao<TResposta>(ex);
            }
        }

        return ApiResult<TResposta>.Falha("Não foi possível concluir a chamada da API.");
    }

    protected static string MontarQuery(Dictionary<string, string?> parametros)
    {
        var lista = parametros
            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value!)}")
            .ToList();

        if (lista.Count == 0)
            return string.Empty;

        return "?" + string.Join("&", lista);
    }

    private static HttpRequestMessage CriarComJson<T>(HttpMethod method, string endpoint, T body)
    {
        var request = new HttpRequestMessage(method, endpoint);
        var json = JsonSerializer.Serialize(body, JsonOptions);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        return request;
    }

    private static bool EhErroRecuperavel(Exception ex) =>
        ex is HttpRequestException or TaskCanceledException;
}
