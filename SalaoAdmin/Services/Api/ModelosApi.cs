namespace SalaoAdmin.Servicos.Api;

internal class ResultadoApiEnvelope
{
    public bool Sucesso { get; set; }
    public string? Mensagem { get; set; }
    public List<string> Erros { get; set; } = [];
}

internal class ResultadoApiEnvelope<T> : ResultadoApiEnvelope
{
    public T? Dados { get; set; }
}

internal class PaginacaoApi<T>
{
    public int Pagina { get; set; }
    public int Tamanho { get; set; }
    public int Total { get; set; }
    public List<T> Itens { get; set; } = [];
}
