using System.Text.Json;
using Microsoft.JSInterop;
using SalaoAdmin.Dtos.Auth;

namespace SalaoAdmin.Servicos.Api;

public class TokenStorageService(IJSRuntime jsRuntime)
{
    private const string TokenKey = "salao.token";
    private const string UsuarioKey = "salao.usuario";

    public async Task SalvarSessaoAsync(LoginRespostaDto resposta)
    {
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, resposta.Token);
        var usuario = JsonSerializer.Serialize(resposta.Funcionario);
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", UsuarioKey, usuario);
    }

    public async Task<string?> ObterTokenAsync()
    {
        return await jsRuntime.InvokeAsync<string?>("localStorage.getItem", TokenKey);
    }

    public async Task<FuncionarioSessaoDto?> ObterUsuarioAsync()
    {
        var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", UsuarioKey);
        if (string.IsNullOrWhiteSpace(json))
            return null;
        return JsonSerializer.Deserialize<FuncionarioSessaoDto>(json);
    }

    public async Task LimparSessaoAsync()
    {
        await jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        await jsRuntime.InvokeVoidAsync("localStorage.removeItem", UsuarioKey);
    }
}
