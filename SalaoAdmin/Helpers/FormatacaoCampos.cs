using System.Globalization;
using System.Text.RegularExpressions;

namespace SalaoAdmin.Utilitarios;

public static partial class FormatacaoCampos
{
    public static string Telefone(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return string.Empty;

        var digitos = SoNumeros(valor);
        return digitos.Length switch
        {
            <= 2 => $"({digitos}",
            <= 6 => $"({digitos[..2]}) {digitos[2..]}",
            <= 10 => $"({digitos[..2]}) {digitos[2..6]}-{digitos[6..]}",
            _ => $"({digitos[..2]}) {digitos[2..7]}-{digitos[7..11]}"
        };
    }

    public static string SoNumeros(string valor) =>
        NaoDigito().Replace(valor, string.Empty);

    public static string Moeda(decimal valor) =>
        valor.ToString("C2", new CultureInfo("pt-BR"));

    public static decimal? MoedaDeTexto(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return null;

        var limpo = texto.Replace("R$", "").Replace(".", "").Replace(",", ".").Trim();
        return decimal.TryParse(limpo, NumberStyles.Any, CultureInfo.InvariantCulture, out var n)
            ? n
            : null;
    }

    public static string MoedaParaCampo(decimal valor) =>
        valor.ToString("N2", new CultureInfo("pt-BR"));

    [GeneratedRegex(@"\D")]
    private static partial Regex NaoDigito();
}

