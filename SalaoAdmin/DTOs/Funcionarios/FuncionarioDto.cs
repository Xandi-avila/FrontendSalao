using System.Text.Json.Serialization;
using SalaoAdmin.Enums;

namespace SalaoAdmin.Dtos.Funcionarios;

public class FuncionarioDto
{
    public Guid Id { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string? Celular { get; set; }
    public string? CPF { get; set; }
    public DateTime? DataAdmissao { get; set; }

    [JsonPropertyName("profissoes")]
    public List<string> Profissoes { get; set; } = [];

    public string Email { get; set; } = string.Empty;
    public DateTime? DataNascimento { get; set; }
    public NivelPermissao NivelPermissao { get; set; }
    public StatusRegistro Status { get; set; }
}

public class FuncionarioCadastroDto
{
    public string NomeCompleto { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string? Celular { get; set; }
    public string? CPF { get; set; }
    public DateTime? DataAdmissao { get; set; }

    [JsonPropertyName("profissoes")]
    public List<string> Profissoes { get; set; } = [];

    public string Email { get; set; } = string.Empty;
    public string? Senha { get; set; }
    public DateTime? DataNascimento { get; set; }
    public NivelPermissao NivelPermissao { get; set; } = NivelPermissao.Profissional;
    public StatusRegistro Status { get; set; } = StatusRegistro.Ativo;
}

public class FuncionarioEdicaoDto : FuncionarioCadastroDto
{
    public Guid Id { get; set; }
}
