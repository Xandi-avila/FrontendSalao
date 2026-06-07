using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using SalaoAdmin.Configuracao;
using SalaoAdmin.Contratos;
using SalaoAdmin.DadosMock;
using ArmazenamentoLocal = SalaoAdmin.DadosMock.ArmazenamentoLocal;
using SalaoAdmin.Servicos.Api;
using SalaoAdmin.Servicos.Mock;
using SalaoAdmin.Servicos.Compartilhados;

namespace SalaoAdmin.Compartilhado;

public static class RegistroServicos
{
    public static IServiceCollection RegistrarServicos(this IServiceCollection servicos, ConfiguracaoApi config)
    {
        servicos.AddSingleton(config);
        servicos.AddSingleton<ArmazenamentoLocal>();

        servicos.AddAuthorizationCore(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });
        servicos.AddScoped<JwtAuthenticationStateProvider>();
        servicos.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());
        servicos.AddScoped<TokenStorageService>();
        servicos.AddScoped<AuthHttpMessageHandler>();
        servicos.AddScoped<ErrorHandlerService>();

        servicos.AddHttpClient<AuthApiService>(client =>
        {
            client.BaseAddress = new Uri(config.UrlBaseApi);
            client.Timeout = TimeSpan.FromSeconds(config.TimeoutSegundos);
        });

        servicos.AddHttpClient("ApiAutenticada", client =>
            {
                client.BaseAddress = new Uri(config.UrlBaseApi);
                client.Timeout = TimeSpan.FromSeconds(config.TimeoutSegundos);
            })
            .AddHttpMessageHandler<AuthHttpMessageHandler>();

        servicos.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiAutenticada"));

        servicos.AddValidatorsFromAssemblyContaining<Program>();

        servicos.AddScoped<IServicoValidacao, ServicoValidacao>();
        servicos.AddScoped<IServicoNotificacao, ServicoNotificacao>();
        servicos.AddScoped<ITratadorErros, TratadorErros>();
        servicos.AddScoped<IPreferenciaTemaServico, PreferenciaTemaServico>();

        servicos.AddScoped<IAuthService, AuthService>();
        servicos.AddScoped<IAgendaServico, AgendaApiService>();
        servicos.AddScoped<IAgendamentoServico, AgendamentoApiService>();
        servicos.AddScoped<IAnaliseServico, AnaliseApiService>();
        servicos.AddScoped<IVendaServico, VendaApiService>();

        if (config.UsarMocks)
        {
            servicos.AddScoped<IFuncionarioServico, FuncionarioServicoMock>();
            servicos.AddScoped<IClienteServico, ClienteServicoMock>();
            servicos.AddScoped<IProdutoServico, ProdutoServicoMock>();
            servicos.AddScoped<ICategoriaServico, CategoriaServicoMock>();
            servicos.AddScoped<IServicoCatalogoServico, ServicoCatalogoServicoMock>();
            servicos.AddScoped<IAgendaServico, AgendaServicoMock>();
            servicos.AddScoped<IAgendamentoServico, AgendamentoServicoMock>();
            servicos.AddScoped<IAnaliseServico, AnaliseServicoMock>();
            servicos.AddScoped<IVendaServico, VendaServicoMock>();
        }
        else
        {
            servicos.AddScoped<IFuncionarioServico, FuncionarioApiService>();
            servicos.AddScoped<IClienteServico, ClienteApiService>();
            servicos.AddScoped<IProdutoServico, ProdutoApiService>();
            servicos.AddScoped<ICategoriaServico, CategoriaApiService>();
            servicos.AddScoped<IServicoCatalogoServico, ServicoApiService>();
        }

        return servicos;
    }
}

