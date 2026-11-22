using SalaoAdmin.Comum;
using SalaoAdmin.Contratos;
using SalaoAdmin.DadosMock;
using SalaoAdmin.Dtos.Categorias;
using SalaoAdmin.Servicos.Base;

namespace SalaoAdmin.Servicos.Mock;

public class CategoriaServicoMock(ArmazenamentoLocal dados) : ServicoMockBase<CategoriaDto>, ICategoriaServico
{
    public async Task<Resultado<ListaPaginada<CategoriaDto>>> ListarAsync(FiltroPaginacao filtro, CancellationToken cancelamento = default)
    {
        var busca = filtro.Busca?.Trim().ToLowerInvariant();
        return await PaginarAsync(
            dados.Categorias,
            filtro,
            c => string.IsNullOrEmpty(busca) || c.Nome.ToLowerInvariant().Contains(busca),
            c => c.Nome);
    }

    public async Task<Resultado<CategoriaDto>> ObterPorIdAsync(Guid id, CancellationToken cancelamento = default)
    {
        await SimularEspera();
        var item = dados.Categorias.FirstOrDefault(c => c.Id == id);
        return item is null
            ? Resultado<CategoriaDto>.Falha("Categoria não encontrada.")
            : Resultado<CategoriaDto>.Ok(item);
    }

    public async Task<Resultado<CategoriaDto>> CriarAsync(CategoriaCadastroDto dto, CancellationToken cancelamento = default)
    {
        await SimularEspera();
        var nomes = dados.Servicos
            .Where(s => dto.ServicoIds.Contains(s.Id))
            .Select(s => s.Nome)
            .ToList();

        var novo = new CategoriaDto
        {
            Id = Guid.NewGuid(),
            Nome = dto.Nome,
            ServicoIds = dto.ServicoIds,
            NomesServicosVinculados = nomes
        };
        dados.Categorias.Add(novo);
        return Resultado<CategoriaDto>.Ok(novo, "Categoria cadastrada.");
    }

    public async Task<Resultado<CategoriaDto>> AtualizarAsync(CategoriaEdicaoDto dto, CancellationToken cancelamento = default)
    {
        await SimularEspera();
        var indice = dados.Categorias.FindIndex(c => c.Id == dto.Id);
        if (indice < 0)
            return Resultado<CategoriaDto>.Falha("Categoria não encontrada.");

        var nomes = dados.Servicos
            .Where(s => dto.ServicoIds.Contains(s.Id))
            .Select(s => s.Nome)
            .ToList();

        dados.Categorias[indice] = new CategoriaDto
        {
            Id = dto.Id,
            Nome = dto.Nome,
            ServicoIds = dto.ServicoIds,
            NomesServicosVinculados = nomes
        };
        return Resultado<CategoriaDto>.Ok(dados.Categorias[indice], "Categoria atualizada.");
    }

    public async Task<Resultado> ExcluirAsync(Guid id, CancellationToken cancelamento = default)
    {
        await SimularEspera();
        var removido = dados.Categorias.RemoveAll(c => c.Id == id);
        return removido > 0
            ? Resultado.Ok("Categoria excluída.")
            : Resultado.Falha("Categoria não encontrada.");
    }
}
