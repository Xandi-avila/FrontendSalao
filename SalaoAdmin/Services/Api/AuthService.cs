using Microsoft.AspNetCore.Components;
using SalaoAdmin.Comum;
using SalaoAdmin.Contratos;
using SalaoAdmin.Dtos.Auth;

namespace SalaoAdmin.Servicos.Api;

public class AuthService(
    AuthApiService authApiService,
    TokenStorageService tokenStorageService,
    JwtAuthenticationStateProvider authStateProvider,
    NavigationManager navigationManager) : IAuthService
{
    public async Task<Resultado<LoginRespostaDto>> LoginAsync(LoginRequestDto request, CancellationToken cancelamento = default)
    {
        var api = await authApiService.LoginAsync(request, cancelamento);
        if (!api.Sucesso || api.Dados is null || string.IsNullOrWhiteSpace(api.Dados.Token))
            return Resultado<LoginRespostaDto>.Falha(api.Mensagem ?? "Falha ao autenticar.");

        await tokenStorageService.SalvarSessaoAsync(api.Dados);
        authStateProvider.NotificarLogin(api.Dados.Token);
        return Resultado<LoginRespostaDto>.Ok(api.Dados, api.Mensagem ?? "Login realizado.");
    }

    public async Task LogoutAsync()
    {
        await tokenStorageService.LimparSessaoAsync();
        authStateProvider.NotificarLogout();
        navigationManager.NavigateTo("/login", forceLoad: false);
    }
}
