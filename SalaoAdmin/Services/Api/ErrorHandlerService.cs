using System.Net;
using System.Net.Http.Json;
using SalaoAdmin.Comum;

namespace SalaoAdmin.Servicos.Api;

public class ErrorHandlerService
{
    public async Task<ApiResult<T>> TratarFalhaHttpAsync<T>(HttpResponseMessage response, CancellationToken cancelamento = default)
    {
        try
        {
            var envelope = await response.Content.ReadFromJsonAsync<ResultadoApiEnvelope>(cancellationToken: cancelamento);
            if (envelope is not null)
            {
                var mensagem = string.IsNullOrWhiteSpace(envelope.Mensagem)
                    ? MensagemPadrao(response.StatusCode)
                    : envelope.Mensagem;
                return ApiResult<T>.Falha(mensagem, envelope.Erros);
            }
        }
        catch
        {
        }

        return ApiResult<T>.Falha(MensagemPadrao(response.StatusCode));
    }

    public ApiResult<T> TratarExcecao<T>(Exception excecao) =>
        ApiResult<T>.Falha(excecao.Message);

    private static string MensagemPadrao(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.Unauthorized => "Sessão expirada ou não autenticada.",
        HttpStatusCode.Forbidden => "Você não possui permissão para esta operação.",
        HttpStatusCode.NotFound => "Recurso não encontrado.",
        HttpStatusCode.Conflict => "Conflito de dados na operação.",
        HttpStatusCode.UnprocessableEntity => "Dados inválidos para processar a requisição.",
        _ => "Erro ao processar requisição com a API."
    };
}
