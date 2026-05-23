namespace SalaoAdmin.Dtos.Categorias;

public class CategoriaDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public IReadOnlyList<Guid> ServicoIds { get; set; } = [];
    public IReadOnlyList<string> NomesServicosVinculados { get; set; } = [];
}

public class CategoriaCadastroDto
{
    public string Nome { get; set; } = string.Empty;
    public List<Guid> ServicoIds { get; set; } = [];
}

public class CategoriaEdicaoDto : CategoriaCadastroDto
{
    public Guid Id { get; set; }
}

