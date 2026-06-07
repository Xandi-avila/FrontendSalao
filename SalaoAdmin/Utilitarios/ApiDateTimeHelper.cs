namespace SalaoAdmin.Utilitarios;

public static class ApiDateTimeHelper
{
    public static DateTime? NormalizarDataNascimento(DateTime? valor) =>
        valor.HasValue ? DateTime.SpecifyKind(valor.Value.Date, DateTimeKind.Utc) : null;

    public static string? FormatarDataNascimentoApi(DateTime? valor)
    {
        var normalizado = NormalizarDataNascimento(valor);
        return normalizado.HasValue ? ApiDateTimeJsonConverter.ParaIsoUtc(normalizado.Value) : null;
    }

    public static string? FormatarDataHoraApi(DateTime? valor)
    {
        if (!valor.HasValue)
            return null;

        var dt = valor.Value;
        if (dt.Kind == DateTimeKind.Unspecified)
            dt = DateTime.SpecifyKind(dt, DateTimeKind.Local);

        return dt.ToUniversalTime().ToString("o");
    }
}
