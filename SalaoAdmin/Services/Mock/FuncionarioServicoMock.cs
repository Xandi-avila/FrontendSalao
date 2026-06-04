using SalaoAdmin.Comum;
using SalaoAdmin.Contratos;
using SalaoAdmin.DadosMock;
using ArmazenamentoLocal = SalaoAdmin.DadosMock.ArmazenamentoLocal;
using SalaoAdmin.Dtos.Funcionarios;
using SalaoAdmin.Servicos.Base;

namespace SalaoAdmin.Servicos.Mock;

public class FuncionarioServicoMock(ArmazenamentoLocal dados) : ServicoMockBase<FuncionarioDto>, IFuncionarioServico
{
    public async Task<Resultado<ListaPaginada<FuncionarioDto>>> ListarAsync(FiltroPaginacao filtro, CancellationToken cancelamento = default)
    {
        var busca = filtro.Busca?.Trim().ToLowerInvariant();
        return await PaginarAsync(
            dados.Funcionarios,
            filtro,
            f => string.IsNullOrEmpty(busca) ||
                 f.NomeCompleto.ToLowerInvariant().Contains(busca) ||
                 f.Email.ToLowerInvariant().Contains(busca) ||
                 f.Celular?.ToLowerInvariant().Contains(busca) == true ||
                 f.CPF?.ToLowerInvariant().Contains(busca) == true ||
                 f.Profissoes.Any(p => p.ToLowerInvariant().Contains(busca)),
            f => f.NomeCompleto);
    }

    public async Task<Resultado<FuncionarioDto>> ObterPorIdAsync(Guid id, CancellationToken cancelamento = default)
    {
        await SimularEspera();
        var item = dados.Funcionarios.FirstOrDefault(f => f.Id == id);
        return item is null
            ? Resultado<FuncionarioDto>.Falha("Funcionário não encontrado.")
            : Resultado<FuncionarioDto>.Ok(item);
    }

    public async Task<Resultado<FuncionarioDto>> CriarAsync(FuncionarioCadastroDto dto, CancellationToken cancelamento = default)
    {
        await SimularEspera();
        var novo = new FuncionarioDto
        {
            Id = Guid.NewGuid(),
            NomeCompleto = dto.NomeCompleto,
            Endereco = dto.Endereco,
            Telefone = dto.Telefone,
            Celular = dto.Celular,
            CPF = dto.CPF,
            DataAdmissao = dto.DataAdmissao,
            Profissoes = dto.Profissoes,
            Email = dto.Email,
            DataNascimento = dto.DataNascimento,
            NivelPermissao = dto.NivelPermissao,
            Status = dto.Status
        };
        dados.Funcionarios.Add(novo);
        return Resultado<FuncionarioDto>.Ok(novo, "Funcionário cadastrado.");
    }

    public async Task<Resultado<FuncionarioDto>> AtualizarAsync(FuncionarioEdicaoDto dto, CancellationToken cancelamento = default)
    {
        await SimularEspera();
        var indice = dados.Funcionarios.FindIndex(f => f.Id == dto.Id);
        if (indice < 0)
            return Resultado<FuncionarioDto>.Falha("Funcionário não encontrado.");

        dados.Funcionarios[indice] = new FuncionarioDto
        {
            Id = dto.Id,
            NomeCompleto = dto.NomeCompleto,
            Endereco = dto.Endereco,
            Telefone = dto.Telefone,
            Celular = dto.Celular,
            CPF = dto.CPF,
            DataAdmissao = dto.DataAdmissao,
            Profissoes = dto.Profissoes,
            Email = dto.Email,
            DataNascimento = dto.DataNascimento,
            NivelPermissao = dto.NivelPermissao,
            Status = dto.Status
        };
        return Resultado<FuncionarioDto>.Ok(dados.Funcionarios[indice], "Dados atualizados.");
    }

    public async Task<Resultado> ExcluirAsync(Guid id, CancellationToken cancelamento = default)
    {
        await SimularEspera();
        var removido = dados.Funcionarios.RemoveAll(f => f.Id == id);
        return removido > 0
            ? Resultado.Ok("Funcionário excluído.")
            : Resultado.Falha("Funcionário não encontrado.");
    }
}


