using SalaoAdmin.Dtos.Vendas;

namespace SalaoAdmin.Utilitarios;

public static class VendaExibicaoHelper
{
    public static string NumeroAmigavel(VendaDto venda) => VendaNumeroHelper.Formatar(venda);

    public static string NumeroAmigavel(int numero) => $"#{numero}";

    [Obsolete("Use NumeroAmigavel(VendaDto) ou NumeroAmigavel(int).")]
    public static string NumeroAmigavel(Guid id) => VendaNumeroHelper.Formatar(null, id);

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
