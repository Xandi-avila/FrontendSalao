namespace SalaoAdmin.Configuracao;

public class ConfiguracaoApi
{
    public const string NomeSecao = "ConfiguracaoApi";

    public string UrlBaseApi { get; set; } = "https://localhost:5001/api/";
    public int TimeoutSegundos { get; set; } = 30;

    public bool UsarMocks { get; set; } = true;
}

