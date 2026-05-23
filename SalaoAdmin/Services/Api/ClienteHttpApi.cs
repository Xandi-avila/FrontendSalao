using System.Net.Http.Json;
using SalaoAdmin.Configuracao;

namespace SalaoAdmin.Servicos.Api;

public class ClienteHttpApi(HttpClient httpClient)
{
    private readonly HttpClient _http = httpClient;

    public async Task<T?> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync(endpoint, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest body,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync(endpoint, body, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(
        string endpoint,
        TRequest body,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PutAsJsonAsync(endpoint, body, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
    }

    public async Task RemoverAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        var response = await _http.DeleteAsync(endpoint, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}

public static class RotasApi
{
    public const string Funcionarios = "funcionarios";
    public const string Clientes = "clientes";
    public const string Produtos = "produtos";
    public const string Categorias = "categorias";
    public const string Servicos = "servicos";
}

