using SalaoAdmin.Enums;

namespace SalaoAdmin.Dtos.Servicos;

public class ServicoDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int DuracaoMinutos { get; set; }
    public decimal PrecoMinimo { get; set; }
    public Guid CategoriaId { get; set; }
    public string NomeCategoria { get; set; } = string.Empty;
    public StatusRegistro Status { get; set; }
}

public class ServicoCadastroDto
{
    public string Nome { get; set; } = string.Empty;
    public int DuracaoMinutos { get; set; }
    public decimal PrecoMinimo { get; set; }
    public Guid CategoriaId { get; set; }
    public StatusRegistro Status { get; set; } = StatusRegistro.Ativo;
}

public class ServicoEdicaoDto : ServicoCadastroDto
{
    public Guid Id { get; set; }
}

