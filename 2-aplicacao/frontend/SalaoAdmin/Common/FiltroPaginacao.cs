namespace SalaoAdmin.Comum;

public class FiltroPaginacao
{
    public int Pagina { get; set; } = 1;
    public int ItensPorPagina { get; set; } = 10;
    public string? Busca { get; set; }
    public string? OrdenarPor { get; set; }
    public bool OrdemDecrescente { get; set; }

    public int Deslocamento => (Pagina - 1) * ItensPorPagina;
}

