using SalaoAdmin.Comum;
using SalaoAdmin.Dtos.Funcionarios;
using SalaoAdmin.Contratos;

namespace SalaoAdmin.Servicos.Api;

public class FuncionarioServicoApi(ClienteHttpApi api) : IFuncionarioServico
{
    private readonly ClienteHttpApi _api = api;

    public Task<Resultado<ListaPaginada<FuncionarioDto>>> ListarAsync(FiltroPaginacao filtro, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Integração com API ainda não implementada. Use FuncionarioServicoMock.");
    }

    public Task<Resultado<FuncionarioDto>> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<Resultado<FuncionarioDto>> CriarAsync(FuncionarioCadastroDto dto, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<Resultado<FuncionarioDto>> AtualizarAsync(FuncionarioEdicaoDto dto, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<Resultado> ExcluirAsync(Guid id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}

