using SalaoAdmin.Comum;
using SalaoAdmin.Dtos.Auth;

namespace SalaoAdmin.Servicos.Api;

public class AuthApiService(
    HttpClient httpClient,
    ErrorHandlerService errorHandler,
    ILogger<AuthApiService> logger)
    : BaseApiService(httpClient, errorHandler, logger)
{
    public async Task<ApiResult<LoginRespostaDto>> LoginAsync(LoginRequestDto request, CancellationToken cancelamento = default)
    {
        return await PostAsync<LoginRequestDto, LoginRespostaDto>("auth/login", request, cancelamento);
    }
}
