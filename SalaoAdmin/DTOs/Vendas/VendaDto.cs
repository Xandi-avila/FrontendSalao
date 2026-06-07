namespace SalaoAdmin.Dtos.Vendas;

public static class TipoItemVenda
{
    public const string Produto = "PRODUTO";
    public const string Servico = "SERVICO";
}

public static class StatusVendaExibicao
{
    public const string Registrada = "REGISTRADA";
    public const string Paga = "PAGA";
}

public class ItemVendaDto
{
    public Guid? Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public Guid? ProdutoId { get; set; }
    public Guid? ServicoId { get; set; }
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
}

public class VendaDto
{
    public Guid Id { get; set; }
    public int? Numero { get; set; }
    public DateTime DataHora { get; set; }
    public decimal Total { get; set; }
    public Guid FuncionarioId { get; set; }
    public Guid? ClienteId { get; set; }
    public List<ItemVendaDto> Itens { get; set; } = [];
}

public class ItemVendaCadastroDto
{
    public string Tipo { get; set; } = string.Empty;
    public Guid? ProdutoId { get; set; }
    public Guid? ServicoId { get; set; }
    public int Quantidade { get; set; } = 1;
    public decimal ValorUnitario { get; set; }
}

public class VendaCadastroDto
{
    public Guid FuncionarioId { get; set; }
    public Guid? ClienteId { get; set; }
    public DateTime? DataHora { get; set; }
    public List<ItemVendaCadastroDto> Itens { get; set; } = [];
}
