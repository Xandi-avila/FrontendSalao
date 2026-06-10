namespace SalaoAdmin.Configuracao;

public class ConfiguracaoApi
{
    public const string NomeSecao = "ConfiguracaoApi";

    public string UrlBaseApi { get; set; } = "/api/";
    public int TimeoutSegundos { get; set; } = 30;

    public bool UsarMocks { get; set; }

    public bool ExigirAutenticacao => true;

    public static string ResolverUrlBaseApi(string urlConfig, string baseAddressSite)
    {
        if (string.IsNullOrWhiteSpace(urlConfig))
            return baseAddressSite;

        if (Uri.TryCreate(urlConfig, UriKind.Absolute, out var absoluta) &&
            (absoluta.Scheme == Uri.UriSchemeHttp || absoluta.Scheme == Uri.UriSchemeHttps))
            return absoluta.AbsoluteUri.EndsWith('/') ? absoluta.AbsoluteUri : absoluta.AbsoluteUri + "/";

        var site = new Uri(baseAddressSite);
        var relativa = urlConfig.StartsWith('/') ? urlConfig : urlConfig + "/";
        var resolvida = new Uri(site, relativa).AbsoluteUri;
        return resolvida.EndsWith('/') ? resolvida : resolvida + "/";
    }
}
