namespace SalaoAdmin.Enums;

public enum NivelPermissao
{
    Recepcao = 1,
    Profissional = 2,
    Gerente = 3,
    Administrador = 4
}

public static class NivelPermissaoExtensions
{
    public static string Descricao(this NivelPermissao nivel) => nivel switch
    {
        NivelPermissao.Recepcao => "Recepção",
        NivelPermissao.Profissional => "Profissional",
        NivelPermissao.Gerente => "Gerente",
        NivelPermissao.Administrador => "Administrador",
        _ => nivel.ToString()
    };
}

