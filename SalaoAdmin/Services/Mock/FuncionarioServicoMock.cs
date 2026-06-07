using SalaoAdmin.Comum;
using SalaoAdmin.Contratos;
using SalaoAdmin.DadosMock;
using ArmazenamentoLocal = SalaoAdmin.DadosMock.ArmazenamentoLocal;
using SalaoAdmin.Dtos.Funcionarios;
using SalaoAdmin.Servicos.Base;
using SalaoAdmin.Utilitarios;

namespace SalaoAdmin.Servicos.Mock;

public class FuncionarioServicoMock(ArmazenamentoLocal dados) : ServicoMockBase<FuncionarioDto>, IFuncionarioServico
{
    public async Task<Resultado<ListaPaginada<FuncionarioDto>>> ListarAsync(FiltroPaginacao filtro, CancellationToken cancelamento = default)
    {
        var busca = filtro.Busca?.Trim();
        return await PaginarAsync(
            dados.Funcionarios,
            filtro,
            f => string.IsNullOrEmpty(busca) ||
                 f.NomeCompleto.Contains(busca, StringComparison.OrdinalIgnoreCase) ||
                 f.Email.Contains(busca, StringComparison.OrdinalIgnoreCase) ||
                 f.Celular?.Contains(busca, StringComparison.OrdinalIgnoreCase) == true ||
                 f.CPF?.Contains(busca, StringComparison.OrdinalIgnoreCase) == true ||
                 ProfissoesHelper.ContemBusca(f.Profissoes, busca),
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
        var novo = Mapear(dto, Guid.NewGuid());
        dados.Funcionarios.Add(novo);
        return Resultado<FuncionarioDto>.Ok(novo, "Funcionário cadastrado.");
    }

    public async Task<Resultado<FuncionarioDto>> AtualizarAsync(FuncionarioEdicaoDto dto, CancellationToken cancelamento = default)
    {
        await SimularEspera();
        var indice = dados.Funcionarios.FindIndex(f => f.Id == dto.Id);
        if (indice < 0)
            return Resultado<FuncionarioDto>.Falha("Funcionário não encontrado.");

        dados.Funcionarios[indice] = Mapear(dto, dto.Id);
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

    private static FuncionarioDto Mapear(FuncionarioCadastroDto dto, Guid id) => new()
    {
        Id = id,
        NomeCompleto = dto.NomeCompleto.Trim(),
        Endereco = dto.Endereco.Trim(),
        Telefone = FormatacaoCampos.Telefone(dto.Telefone),
        Celular = string.IsNullOrWhiteSpace(dto.Celular) ? null : FormatacaoCampos.Telefone(dto.Celular),
        CPF = FormatacaoCampos.CpfSomenteDigitos(dto.CPF),
        DataAdmissao = dto.DataAdmissao,
        Profissoes = ProfissoesHelper.Limpar(dto.Profissoes),
        Email = dto.Email.Trim(),
        DataNascimento = dto.DataNascimento,
        NivelPermissao = dto.NivelPermissao,
        Status = dto.Status
    };
}
