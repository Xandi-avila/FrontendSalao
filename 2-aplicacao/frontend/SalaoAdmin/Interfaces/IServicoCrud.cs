using SalaoAdmin.Comum;

namespace SalaoAdmin.Contratos;

public interface IServicoCrud<TDto, TCadastro, TEdicao>
    where TDto : class
    where TCadastro : class
    where TEdicao : class
{
    Task<Resultado<ListaPaginada<TDto>>> ListarAsync(FiltroPaginacao filtro, CancellationToken cancelamento = default);
    Task<Resultado<TDto>> ObterPorIdAsync(Guid id, CancellationToken cancelamento = default);
    Task<Resultado<TDto>> CriarAsync(TCadastro dto, CancellationToken cancelamento = default);
    Task<Resultado<TDto>> AtualizarAsync(TEdicao dto, CancellationToken cancelamento = default);
    Task<Resultado> ExcluirAsync(Guid id, CancellationToken cancelamento = default);
}

