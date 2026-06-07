using System.Globalization;
using System.Text.RegularExpressions;

namespace SalaoAdmin.Utilitarios;

public static partial class FormatacaoCampos
{
    public static string Telefone(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return string.Empty;

        var d = SoNumeros(valor);
        if (d.Length == 0)
            return string.Empty;

        if (d.Length <= 2)
            return $"({d}";

        if (d.Length <= 6)
            return $"({d[..2]}) {d[2..]}";

        if (d.Length <= 10)
            return $"({d[..2]}) {d[2..6]}-{d[6..]}";

        // Celular: no máximo 11 dígitos (DDD + 9 números)
        d = d.Length > 11 ? d[..11] : d;
        return $"({d[..2]}) {d[2..7]}-{d[7..]}";
    }

    public static string SoNumeros(string valor) =>
        NaoDigito().Replace(valor, string.Empty);

    public static string Cpf(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return string.Empty;

        var d = SoNumeros(valor);
        if (d.Length == 0)
            return string.Empty;

        if (d.Length > 11)
            d = d[..11];

        if (d.Length <= 3)
            return d;

        if (d.Length <= 6)
            return $"{d[..3]}.{d[3..]}";

        if (d.Length <= 9)
            return $"{d[..3]}.{d[3..6]}.{d[6..]}";

        return $"{d[..3]}.{d[3..6]}.{d[6..9]}-{d[9..]}";
    }

    public static string? CpfSomenteDigitos(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : SoNumeros(valor);

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

    public static decimal MoedaValorDeDigitos(string? texto)
    {
        var digitos = SoNumeros(texto ?? string.Empty);
        if (digitos.Length == 0)
            return 0;

        if (digitos.Length > 15)
            digitos = digitos[^15..];

        return long.TryParse(digitos, out var centavos)
            ? centavos / 100m
            : 0;
    }

    public static string Data(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return string.Empty;

        var d = SoNumeros(valor);
        if (d.Length == 0)
            return string.Empty;

        if (d.Length > 8)
            d = d[..8];

        if (d.Length <= 2)
            return d;

        if (d.Length <= 4)
            return $"{d[..2]}/{d[2..]}";

        return $"{d[..2]}/{d[2..4]}/{d[4..]}";
    }

    public static string DataParaCampo(DateTime? valor) =>
        valor?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? string.Empty;

    public static DateTime? DataDeTexto(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return null;

        var d = SoNumeros(texto);
        if (d.Length != 8)
            return null;

        return DateTime.TryParseExact(
            d,
            "ddMMyyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var dt)
            ? dt.Date
            : null;
    }

    [GeneratedRegex(@"\D")]
    private static partial Regex NaoDigito();
}
