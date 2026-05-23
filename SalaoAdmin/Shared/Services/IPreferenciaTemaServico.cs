using Microsoft.JSInterop;

namespace SalaoAdmin.Servicos.Compartilhados;

public interface IPreferenciaTemaServico
{
    Task<bool> ObterModoEscuroAsync();
    Task SalvarModoEscuroAsync(bool modoEscuro);
}

public class PreferenciaTemaServico(IJSRuntime jsRuntime) : IPreferenciaTemaServico
{
    public Task<bool> ObterModoEscuroAsync() =>
        jsRuntime.InvokeAsync<bool>("salaoAdminTema.obter").AsTask();

    public Task SalvarModoEscuroAsync(bool modoEscuro) =>
        jsRuntime.InvokeVoidAsync("salaoAdminTema.salvar", modoEscuro).AsTask();
}
