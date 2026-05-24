using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;

namespace SalaoAdmin.Servicos.Api;

public class AuthHttpMessageHandler(
    TokenStorageService tokenStorageService,
    NavigationManager navigationManager) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await tokenStorageService.ObterTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await tokenStorageService.LimparSessaoAsync();
            navigationManager.NavigateTo("/login", forceLoad: false);
        }

        return response;
    }
}
