using FluentValidation;
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

        servicos.AddScoped(_ =>
        {
            var http = new HttpClient
            {
                BaseAddress = new Uri(config.UrlBaseApi),
                Timeout = TimeSpan.FromSeconds(config.TimeoutSegundos)
            };
            return http;
        });
        servicos.AddScoped<ClienteHttpApi>();

        servicos.AddValidatorsFromAssemblyContaining<Program>();

        servicos.AddScoped<IServicoValidacao, ServicoValidacao>();
        servicos.AddScoped<IServicoNotificacao, ServicoNotificacao>();
        servicos.AddScoped<ITratadorErros, TratadorErros>();
        servicos.AddScoped<IPreferenciaTemaServico, PreferenciaTemaServico>();

        if (config.UsarMocks)
        {
            servicos.AddScoped<IFuncionarioServico, FuncionarioServicoMock>();
            servicos.AddScoped<IClienteServico, ClienteServicoMock>();
            servicos.AddScoped<IProdutoServico, ProdutoServicoMock>();
            servicos.AddScoped<ICategoriaServico, CategoriaServicoMock>();
            servicos.AddScoped<IServicoCatalogoServico, ServicoCatalogoServicoMock>();
        }
        else
        {
            servicos.AddScoped<IFuncionarioServico, FuncionarioServicoApi>();
        }

        return servicos;
    }
}

