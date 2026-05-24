using SalaoAdmin.Enums;

namespace SalaoAdmin.Dtos.Auth;

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class LoginRespostaDto
{
    public string Token { get; set; } = string.Empty;
    public FuncionarioSessaoDto? Funcionario { get; set; }
}

public class FuncionarioSessaoDto
{
    public Guid Id { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public NivelPermissao NivelPermissao { get; set; }
}
