namespace SalaoAdmin.Dtos.Clientes;

public class ClienteDto
{
    public Guid Id { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string WhatsApp { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Instagram { get; set; }
    public string? Facebook { get; set; }
    public List<string> Profissoes { get; set; } = new();
    public DateTime? DataNascimento { get; set; }
    public string Endereco { get; set; } = string.Empty;
}

public class ClienteCadastroDto
{
    public string NomeCompleto { get; set; } = string.Empty;
    public string WhatsApp { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Instagram { get; set; }
    public string? Facebook { get; set; }
    public List<string> Profissoes { get; set; } = new();
    public DateTime? DataNascimento { get; set; }
    public string Endereco { get; set; } = string.Empty;
}

public class ClienteEdicaoDto : ClienteCadastroDto
{
    public Guid Id { get; set; }
}