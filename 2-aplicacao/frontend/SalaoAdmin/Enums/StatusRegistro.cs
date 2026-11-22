namespace SalaoAdmin.Enums;

public enum StatusRegistro
{
    Inativo = 0,
    Ativo = 1
}

public static class StatusRegistroExtensions
{
    public static string Descricao(this StatusRegistro status) => status switch
    {
        StatusRegistro.Ativo => "Ativo",
        StatusRegistro.Inativo => "Inativo",
        _ => status.ToString()
    };
}

