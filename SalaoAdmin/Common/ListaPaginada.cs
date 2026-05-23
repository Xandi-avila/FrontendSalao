namespace SalaoAdmin.Comum;

public class ListaPaginada<T>
{
    public IReadOnlyList<T> Itens { get; init; } = [];
    public int Total { get; init; }
    public int Pagina { get; init; }
    public int ItensPorPagina { get; init; }
    public int TotalPaginas => ItensPorPagina > 0 ? (int)Math.Ceiling(Total / (double)ItensPorPagina) : 0;
    public bool TemAnterior => Pagina > 1;
    public bool TemProxima => Pagina < TotalPaginas;
}

