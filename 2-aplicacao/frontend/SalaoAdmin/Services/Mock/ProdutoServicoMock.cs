using SalaoAdmin.Comum;
using SalaoAdmin.Contratos;
using SalaoAdmin.DadosMock;
using SalaoAdmin.Dtos.Produtos;
using SalaoAdmin.Servicos.Base;

namespace SalaoAdmin.Servicos.Mock;

public class ProdutoServicoMock(ArmazenamentoLocal dados) : ServicoMockBase<ProdutoDto>, IProdutoServico
{
    public async Task<Resultado<ListaPaginada<ProdutoDto>>> ListarAsync(FiltroPaginacao filtro, CancellationToken cancelamento = default)
    {
        var busca = filtro.Busca?.Trim().ToLowerInvariant();
        return await PaginarAsync(
            dados.Produtos,
            filtro,
            p => string.IsNullOrEmpty(busca) || p.Nome.ToLowerInvariant().Contains(busca),
            p => p.Nome);
    }

    public async Task<Resultado<ProdutoDto>> ObterPorIdAsync(Guid id, CancellationToken cancelamento = default)
    {
        await SimularEspera();
        var item = dados.Produtos.FirstOrDefault(p => p.Id == id);
        return item is null
            ? Resultado<ProdutoDto>.Falha("Produto não encontrado.")
            : Resultado<ProdutoDto>.Ok(item);
    }

    public async Task<Resultado<ProdutoDto>> CriarAsync(ProdutoCadastroDto dto, CancellationToken cancelamento = default)
    {
        await SimularEspera();
        var novo = new ProdutoDto
        {
            Id = Guid.NewGuid(),
            Nome = dto.Nome,
            Valor = dto.Valor,
            CaminhoImagem = dto.CaminhoImagem,
            Status = dto.Status
        };
        dados.Produtos.Add(novo);
        return Resultado<ProdutoDto>.Ok(novo, "Produto cadastrado.");
    }

    public async Task<Resultado<ProdutoDto>> AtualizarAsync(ProdutoEdicaoDto dto, CancellationToken cancelamento = default)
    {
        await SimularEspera();
        var indice = dados.Produtos.FindIndex(p => p.Id == dto.Id);
        if (indice < 0)
            return Resultado<ProdutoDto>.Falha("Produto não encontrado.");

        dados.Produtos[indice] = new ProdutoDto
        {
            Id = dto.Id,
            Nome = dto.Nome,
            Valor = dto.Valor,
            CaminhoImagem = dto.CaminhoImagem,
            Status = dto.Status
        };
        return Resultado<ProdutoDto>.Ok(dados.Produtos[indice], "Produto atualizado.");
    }

    public async Task<Resultado> ExcluirAsync(Guid id, CancellationToken cancelamento = default)
    {
        await SimularEspera();
        var removido = dados.Produtos.RemoveAll(p => p.Id == id);
        return removido > 0
            ? Resultado.Ok("Produto excluído.")
            : Resultado.Falha("Produto não encontrado.");
    }
}
