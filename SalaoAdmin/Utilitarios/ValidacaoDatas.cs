namespace SalaoAdmin.Utilitarios;

public static class ValidacaoDatas
{
    public static readonly DateTime DataMinimaPadrao = new(1900, 1, 1);

    public static bool NaoFutura(DateTime? data) =>
        !data.HasValue || data.Value.Date <= DateTime.Today;

    public static bool AcimaDoMinimo(DateTime? data) =>
        !data.HasValue || data.Value.Date > DataMinimaPadrao;

    public static string? MensagemErroNascimento(DateTime? data)
    {
        if (!data.HasValue)
            return null;

        if (!AcimaDoMinimo(data))
            return "Data deve ser posterior a 01/01/1900.";

        if (!NaoFutura(data))
            return "Data de nascimento não pode ser futura.";

        return null;
    }

    public static string? MensagemErroAdmissao(DateTime? data)
    {
        if (!data.HasValue)
            return "Informe a data de admissão.";

        if (!AcimaDoMinimo(data))
            return "Data deve ser posterior a 01/01/1900.";

        if (!NaoFutura(data))
            return "Data de admissão não pode ser futura.";

        return null;
    }

    public static string? ValidarPeriodo(DateTime? data, DateTime? minima, DateTime? maxima)
    {
        if (!data.HasValue)
            return null;

        if (minima.HasValue && data.Value.Date <= minima.Value.Date)
            return "Data deve ser posterior a 01/01/1900.";

        if (maxima.HasValue && data.Value.Date > maxima.Value.Date)
            return "Data não pode ser futura.";

        return null;
    }
}
