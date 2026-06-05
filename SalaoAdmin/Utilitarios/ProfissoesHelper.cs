namespace SalaoAdmin.Utilitarios;

public static class ProfissoesHelper
{
    public static List<string> Parse(string? texto) =>
        string.IsNullOrWhiteSpace(texto)
            ? []
            : texto.Split(',')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

    public static string ParaTexto(IEnumerable<string>? profissoes) =>
        profissoes is null ? string.Empty : string.Join(", ", profissoes);

    public static string Exibir(IEnumerable<string>? profissoes) =>
        profissoes is null || !profissoes.Any() ? "—" : string.Join(", ", profissoes);

    public static bool ContemBusca(IEnumerable<string>? profissoes, string busca) =>
        profissoes?.Any(p => p.Contains(busca, StringComparison.OrdinalIgnoreCase)) == true;

    public static List<string> Limpar(IEnumerable<string>? profissoes) =>
        profissoes?
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? [];
}
