using SalaoAdmin.Comum;
using SalaoAdmin.Contratos;
using SalaoAdmin.DadosMock;
using SalaoAdmin.Dtos.Clientes;
using SalaoAdmin.Servicos.Base;
using SalaoAdmin.Utilitarios;

namespace SalaoAdmin.Servicos.Mock;

public class ClienteServicoMock(ArmazenamentoLocal dados) : ServicoMockBase<ClienteDto>, IClienteServico
{
    public async Task<Resultado<ListaPaginada<ClienteDto>>> ListarAsync(FiltroPaginacao filtro, CancellationToken cancelamento = default)
    {
        var busca = filtro.Busca?.Trim();
        return await PaginarAsync(
            dados.Clientes,
            filtro,
            c => string.IsNullOrEmpty(busca) ||
                 c.NomeCompleto.Contains(busca, StringComparison.OrdinalIgnoreCase) ||
                 c.WhatsApp.Contains(busca, StringComparison.OrdinalIgnoreCase) ||
                 c.Email?.Contains(busca, StringComparison.OrdinalIgnoreCase) == true ||
                 c.Instagram?.Contains(busca, StringComparison.OrdinalIgnoreCase) == true ||
                 ProfissoesHelper.ContemBusca(c.Profissao, busca) ||
                 c.Endereco.Contains(busca, StringComparison.OrdinalIgnoreCase),
            c => c.NomeCompleto);
    }

    public async Task<Resultado<ClienteDto>> ObterPorIdAsync(Guid id, CancellationToken cancelamento = default)
    {
        await SimularEspera();
        var item = dados.Clientes.FirstOrDefault(c => c.Id == id);
        return item is null
            ? Resultado<ClienteDto>.Falha("Cliente não encontrado.")
            : Resultado<ClienteDto>.Ok(item);
    }

    public async Task<Resultado<ClienteDto>> CriarAsync(ClienteCadastroDto dto, CancellationToken cancelamento = default)
    {
        await SimularEspera();
        var novo = Mapear(dto, Guid.NewGuid());
        dados.Clientes.Add(novo);
        return Resultado<ClienteDto>.Ok(novo, "Cliente cadastrado.");
    }

    public async Task<Resultado<ClienteDto>> AtualizarAsync(ClienteEdicaoDto dto, CancellationToken cancelamento = default)
    {
        await SimularEspera();
        var indice = dados.Clientes.FindIndex(c => c.Id == dto.Id);
        if (indice < 0)
            return Resultado<ClienteDto>.Falha("Cliente não encontrado.");

        dados.Clientes[indice] = Mapear(dto, dto.Id);
        return Resultado<ClienteDto>.Ok(dados.Clientes[indice], "Cliente atualizado.");
    }

    public async Task<Resultado> ExcluirAsync(Guid id, CancellationToken cancelamento = default)
    {
        await SimularEspera();
        var removido = dados.Clientes.RemoveAll(c => c.Id == id);
        return removido > 0
            ? Resultado.Ok("Cliente excluído.")
            : Resultado.Falha("Cliente não encontrado.");
    }

    private static ClienteDto Mapear(ClienteCadastroDto dto, Guid id) => new()
    {
        Id = id,
        NomeCompleto = dto.NomeCompleto.Trim(),
        WhatsApp = FormatacaoCampos.Telefone(dto.WhatsApp),
        Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
        Instagram = string.IsNullOrWhiteSpace(dto.Instagram) ? null : dto.Instagram.Trim(),
        Profissao = ProfissoesHelper.Limpar(dto.Profissao),
        DataNascimento = dto.DataNascimento,
        Endereco = dto.Endereco.Trim()
    };
}
