namespace SalaoAdmin.Configuracao;

public class ConfiguracaoApi
{
    public const string NomeSecao = "ConfiguracaoApi";

    public string UrlBaseApi { get; set; } = "https://salao-beleza-service.onrender.com/";
    public int TimeoutSegundos { get; set; } = 30;

    public bool UsarMocks { get; set; } = true;

    public bool ExigirAutenticacao => true;
}

