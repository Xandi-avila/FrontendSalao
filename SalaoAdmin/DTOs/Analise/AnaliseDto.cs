namespace SalaoAdmin.Dtos.Analise;

public class AnaliseFuncionarioItemDto
{
    public Guid? FuncionarioId { get; set; }
    public string? Nome { get; set; }
    public string? NomeFuncionario { get; set; }
    public int Quantidade { get; set; }
    public int Total { get; set; }
    public decimal Faturamento { get; set; }
}

public class AnaliseServicoItemDto
{
    public Guid? ServicoId { get; set; }
    public string? Nome { get; set; }
    public string? NomeServico { get; set; }
    public int Quantidade { get; set; }
    public int Total { get; set; }
}

public class AnaliseFaturamentoDto
{
    public decimal Total { get; set; }
    public decimal Faturamento { get; set; }
    public decimal Valor { get; set; }
}
