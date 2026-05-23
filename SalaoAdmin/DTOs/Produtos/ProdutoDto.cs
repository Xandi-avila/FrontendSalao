using SalaoAdmin.Enums;

namespace SalaoAdmin.Dtos.Produtos;

public class ProdutoDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string? CaminhoImagem { get; set; }
    public StatusRegistro Status { get; set; }
}

public class ProdutoCadastroDto
{
    public string Nome { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string? CaminhoImagem { get; set; }
    public StatusRegistro Status { get; set; } = StatusRegistro.Ativo;
}

public class ProdutoEdicaoDto : ProdutoCadastroDto
{
    public Guid Id { get; set; }
}

