using SalaoAdmin.Comum;
using SalaoAdmin.Dtos.Auth;

namespace SalaoAdmin.Contratos;

public interface IAuthService
{
    Task<Resultado<LoginRespostaDto>> LoginAsync(LoginRequestDto request, CancellationToken cancelamento = default);
    Task LogoutAsync();
}
