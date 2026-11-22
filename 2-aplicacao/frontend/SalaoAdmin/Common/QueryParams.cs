namespace SalaoAdmin.Comum;

public class QueryParams
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public bool Desc { get; set; }

    public Dictionary<string, string> Extras { get; } = [];
}
