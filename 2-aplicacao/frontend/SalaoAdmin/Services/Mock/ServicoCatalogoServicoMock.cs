using SalaoAdmin.Comum;
using SalaoAdmin.Contratos;
using SalaoAdmin.DadosMock;
using SalaoAdmin.Dtos.Servicos;
using SalaoAdmin.Servicos.Base;

namespace SalaoAdmin.Servicos.Mock;

public class ServicoCatalogoServicoMock(ArmazenamentoLocal dados) : ServicoMockBase<ServicoDto>, IServicoCatalogoServico
{
    public async Task<Resultado<ListaPaginada<ServicoDto>>> ListarAsync(FiltroPaginacao filtro, CancellationToken cancelamento = default)
    {
        var busca = filtro.Busca?.Trim().ToLowerInvariant();
        return await PaginarAsync(
            dados.Servicos,
            filtro,
            s => string.IsNullOrEmpty(busca) ||
                 s.Nome.ToLowerInvariant().Contains(busca) ||
                 s.NomeCategoria.ToLowerInvariant().Contains(busca),
            s => s.Nome);
    }

    public async Task<Resultado<ServicoDto>> ObterPorIdAsync(Guid id, CancellationToken cancelamento = default)
    {
        await SimularEspera();
        var item = dados.Servicos.FirstOrDefault(s => s.Id == id);
        return item is null
            ? Resultado<ServicoDto>.Falha("Serviço não encontrado.")
            : Resultado<ServicoDto>.Ok(item);
    }

    public async Task<Resultado<ServicoDto>> CriarAsync(ServicoCadastroDto dto, CancellationToken cancelamento = default)
    {
        await SimularEspera();
        var categoria = dados.Categorias.FirstOrDefault(c => c.Id == dto.CategoriaId);
        var novo = new ServicoDto
        {
            Id = Guid.NewGuid(),
            Nome = dto.Nome,
            DuracaoMinutos = dto.DuracaoMinutos,
            PrecoMinimo = dto.PrecoMinimo,
            CategoriaId = dto.CategoriaId,
            NomeCategoria = categoria?.Nome ?? "—",
            Status = dto.Status
        };
        dados.Servicos.Add(novo);
        return Resultado<ServicoDto>.Ok(novo, "Serviço cadastrado.");
    }

    public async Task<Resultado<ServicoDto>> AtualizarAsync(ServicoEdicaoDto dto, CancellationToken cancelamento = default)
    {
        await SimularEspera();
        var indice = dados.Servicos.FindIndex(s => s.Id == dto.Id);
        if (indice < 0)
            return Resultado<ServicoDto>.Falha("Serviço não encontrado.");

        var categoria = dados.Categorias.FirstOrDefault(c => c.Id == dto.CategoriaId);
        dados.Servicos[indice] = new ServicoDto
        {
            Id = dto.Id,
            Nome = dto.Nome,
            DuracaoMinutos = dto.DuracaoMinutos,
            PrecoMinimo = dto.PrecoMinimo,
            CategoriaId = dto.CategoriaId,
            NomeCategoria = categoria?.Nome ?? "—",
            Status = dto.Status
        };
        return Resultado<ServicoDto>.Ok(dados.Servicos[indice], "Serviço atualizado.");
    }

    public async Task<Resultado> ExcluirAsync(Guid id, CancellationToken cancelamento = default)
    {
        await SimularEspera();
        var removido = dados.Servicos.RemoveAll(s => s.Id == id);
        return removido > 0
            ? Resultado.Ok("Serviço excluído.")
            : Resultado.Falha("Serviço não encontrado.");
    }
}
