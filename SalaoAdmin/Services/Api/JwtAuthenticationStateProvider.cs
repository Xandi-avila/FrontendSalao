using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace SalaoAdmin.Servicos.Api;

public class JwtAuthenticationStateProvider(TokenStorageService tokenStorageService) : AuthenticationStateProvider
{
    private static readonly AuthenticationState EstadoAnonimo =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await tokenStorageService.ObterTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
            return EstadoAnonimo;

        var identidade = CriarIdentidade(token);
        if (identidade is null)
            return EstadoAnonimo;

        return new AuthenticationState(new ClaimsPrincipal(identidade));
    }

    public void NotificarLogin(string token)
    {
        var identidade = CriarIdentidade(token);
        if (identidade is null)
        {
            NotificarLogout();
            return;
        }

        var estado = new AuthenticationState(new ClaimsPrincipal(identidade));
        NotifyAuthenticationStateChanged(Task.FromResult(estado));
    }

    public void NotificarLogout()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(EstadoAnonimo));
    }

    private static ClaimsIdentity? CriarIdentidade(string token)
    {
        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            if (jwt.ValidTo <= DateTime.UtcNow)
                return null;

            var claims = jwt.Claims.ToList();
            var nivel = claims.FirstOrDefault(c => c.Type is "nivelPermissao" or "nivel_permissao")?.Value;
            if (!string.IsNullOrWhiteSpace(nivel))
                claims.Add(new Claim(ClaimTypes.Role, nivel));

            return new ClaimsIdentity(claims, "jwt");
        }
        catch
        {
            return null;
        }
    }
}
