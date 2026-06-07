using SalaoAdmin.Comum;
using SalaoAdmin.Contratos;
using SalaoAdmin.Dtos.Vendas;

namespace SalaoAdmin.Utilitarios;

public static class VendaNumeroHelper
{
    public static string Formatar(VendaDto venda) =>
        Formatar(venda.Numero, venda.Id);

    public static string Formatar(int? numero, Guid id) =>
        numero is > 0 ? $"#{numero}" : $"#{id.ToString("N")[..8].ToUpperInvariant()}";

    public static int CalcularNaPaginaDesc(int total, int pagina, int itensPorPagina, int indiceLinha) =>
        total - ((pagina - 1) * itensPorPagina + indiceLinha);

    public static bool TryParseNumeroBusca(string? busca, out int numero)
    {
        numero = 0;
        if (string.IsNullOrWhiteSpace(busca))
            return false;

        var termo = busca.Trim();
        if (termo.StartsWith('#'))
            termo = termo[1..].Trim();

        return int.TryParse(termo, out numero) && numero > 0;
    }

    public static Dictionary<Guid, int> ConstruirMapaNumeros(IEnumerable<VendaDto> vendas) =>
        vendas
            .OrderBy(v => v.DataHora)
            .ThenBy(v => v.Id)
            .Select((v, i) => (v.Id, Numero: v.Numero is > 0 ? v.Numero.Value : i + 1))
            .ToDictionary(x => x.Id, x => x.Numero);

    public static HashSet<Guid> BuscarIdsPorNumero(IEnumerable<VendaDto> vendas, int numero)
    {
        var mapa = ConstruirMapaNumeros(vendas);
        return mapa.Where(kv => kv.Value == numero).Select(kv => kv.Key).ToHashSet();
    }

    public static int ObterNumero(VendaDto venda, IReadOnlyDictionary<Guid, int> mapa) =>
        venda.Numero is > 0
            ? venda.Numero.Value
            : mapa.TryGetValue(venda.Id, out var numero) ? numero : 0;

    public static async Task<Dictionary<Guid, int>> MontarMapaGlobalAsync(
        IVendaServico servico,
        CancellationToken cancelamento = default)
    {
        var todos = new List<VendaDto>();
        var pagina = 1;

        while (true)
        {
            var resposta = await servico.ListarAsync(new FiltroVenda
            {
                Pagina = pagina,
                ItensPorPagina = 100
            }, cancelamento);

            if (!resposta.Sucesso || resposta.Dados is null)
                break;

            todos.AddRange(resposta.Dados.Itens);
            if (todos.Count >= resposta.Dados.Total || resposta.Dados.Itens.Count < 100)
                break;

            pagina++;
        }

        return todos
            .OrderBy(v => v.DataHora)
            .ThenBy(v => v.Id)
            .Select((v, i) => (v.Id, Numero: i + 1))
            .ToDictionary(x => x.Id, x => x.Numero);
    }

    public static void AplicarNumerosMock(IReadOnlyList<VendaDto> vendas)
    {
        var ordenadas = vendas.OrderBy(v => v.DataHora).ThenBy(v => v.Id).ToList();
        for (var i = 0; i < ordenadas.Count; i++)
            ordenadas[i].Numero = i + 1;
    }
}
