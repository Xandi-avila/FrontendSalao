namespace SalaoAdmin.Utilitarios;

public static class VendaExibicaoHelper
{
    public static string NumeroAmigavel(Guid id) =>
        $"#{id.ToString("N")[..8].ToUpperInvariant()}";

    public static string TextoStatus() => "Registrada";

    public static string TextoTipoItem(string tipo) => tipo.ToUpperInvariant() switch
    {
        "PRODUTO" => "Produto",
        "SERVICO" => "Serviço",
        _ => tipo
    };

    public static ColorCorStatus CorStatus() => ColorCorStatus.Sucesso;

    public enum ColorCorStatus { Sucesso, Info, Padrao }
}
