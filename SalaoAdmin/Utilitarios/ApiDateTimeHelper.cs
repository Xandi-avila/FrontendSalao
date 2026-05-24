namespace SalaoAdmin.Utilitarios;

public static class ApiDateTimeHelper
{
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
