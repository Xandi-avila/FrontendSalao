using SalaoAdmin.Comum;
using SalaoAdmin.Contratos;
using SalaoAdmin.DadosMock;
using SalaoAdmin.Dtos.Clientes;
using SalaoAdmin.Servicos.Base;

namespace SalaoAdmin.Servicos.Mock;

public class ClienteServicoMock(ArmazenamentoLocal dados) : ServicoMockBase<ClienteDto>, IClienteServico
{
    public async Task<Resultado<ListaPaginada<ClienteDto>>> ListarAsync(FiltroPaginacao filtro, CancellationToken cancelamento = default)
    {
        var busca = filtro.Busca?.Trim().ToLowerInvariant();
        return await PaginarAsync(
            dados.Clientes,
            filtro,
            c => string.IsNullOrEmpty(busca) ||
                 c.NomeCompleto.ToLowerInvariant().Contains(busca) ||
                 c.WhatsApp.Contains(busca) ||
                 c.Email?.ToLowerInvariant().Contains(busca) == true ||
                 c.Instagram?.ToLowerInvariant().Contains(busca) == true ||
                 c.Facebook?.ToLowerInvariant().Contains(busca) == true ||
                 c.Profissoes.Any(p => p.ToLowerInvariant().Contains(busca)) ||
                 c.Endereco.ToLowerInvariant().Contains(busca),
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
        var novo = new ClienteDto
        {
            Id = Guid.NewGuid(),
            NomeCompleto = dto.NomeCompleto,
            WhatsApp = dto.WhatsApp,
            Email = dto.Email,
            Instagram = dto.Instagram,
            Facebook = dto.Facebook,
            Profissoes = dto.Profissoes,
            DataNascimento = dto.DataNascimento,
            Endereco = dto.Endereco
        };
        dados.Clientes.Add(novo);
        return Resultado<ClienteDto>.Ok(novo, "Cliente cadastrado.");
    }

    public async Task<Resultado<ClienteDto>> AtualizarAsync(ClienteEdicaoDto dto, CancellationToken cancelamento = default)
    {
        await SimularEspera();
        var indice = dados.Clientes.FindIndex(c => c.Id == dto.Id);
        if (indice < 0)
            return Resultado<ClienteDto>.Falha("Cliente não encontrado.");

        dados.Clientes[indice] = new ClienteDto
        {
            Id = dto.Id,
            NomeCompleto = dto.NomeCompleto,
            WhatsApp = dto.WhatsApp,
            Email = dto.Email,
            Instagram = dto.Instagram,
            Facebook = dto.Facebook,
            Profissoes = dto.Profissoes,
            DataNascimento = dto.DataNascimento,
            Endereco = dto.Endereco
        };
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
}
