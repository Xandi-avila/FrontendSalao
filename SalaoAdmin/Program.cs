using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using SalaoAdmin;
using SalaoAdmin.Compartilhado;
using SalaoAdmin.Configuracao;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var configApi = new ConfiguracaoApi();
builder.Configuration.GetSection(ConfiguracaoApi.NomeSecao).Bind(configApi);
configApi.UrlBaseApi = ConfiguracaoApi.ResolverUrlBaseApi(configApi.UrlBaseApi, builder.HostEnvironment.BaseAddress);

builder.Services.AddMudServices(c =>
{
    c.SnackbarConfiguration.PositionClass = "mud-snackbar-location-top-right";
    c.SnackbarConfiguration.PreventDuplicates = true;
    c.SnackbarConfiguration.ShowCloseIcon = true;
});

builder.Services.RegistrarServicos(configApi);

await builder.Build().RunAsync();

