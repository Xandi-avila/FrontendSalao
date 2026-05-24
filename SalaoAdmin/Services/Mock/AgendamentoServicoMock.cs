using SalaoAdmin.Comum;
using SalaoAdmin.Contratos;
using SalaoAdmin.DadosMock;
using SalaoAdmin.Dtos.Agendamentos;
using SalaoAdmin.Servicos.Base;

namespace SalaoAdmin.Servicos.Mock;

public class AgendamentoServicoMock(ArmazenamentoLocal dados) : ServicoMockBase<AgendamentoDto>, IAgendamentoServico
{
    public async Task<Resultado<ListaPaginada<AgendamentoDto>>> ListarAsync(FiltroPaginacao filtro, CancellationToken cancelamento = default)
    {
        var agendamentoFiltro = filtro as FiltroAgendamento;
        var busca = filtro.Busca?.Trim().ToLowerInvariant();
        return await PaginarAsync(
            dados.Agendamentos,
            filtro,
            a =>
            {
                if (!string.IsNullOrEmpty(busca) && !a.Status.ToLowerInvariant().Contains(busca))
                    return false;
                if (agendamentoFiltro?.FuncionarioId is Guid funcId && a.FuncionarioId != funcId)
                    return false;
                if (agendamentoFiltro?.ClienteId is Guid cliId && a.ClienteId != cliId)
                    return false;
                if (agendamentoFiltro?.Inicio is DateTime inicio && a.DataHoraInicio < inicio)
                    return false;
                if (agendamentoFiltro?.Fim is DateTime fim && a.DataHoraInicio > fim)
                    return false;
                return true;
            },
            a => a.DataHoraInicio);
    }

    public async Task<Resultado<AgendamentoDto>> ObterPorIdAsync(Guid id, CancellationToken cancelamento = default)
    {
        await SimularEspera();
        var item = dados.Agendamentos.FirstOrDefault(x => x.Id == id);
        return item is null
            ? Resultado<AgendamentoDto>.Falha("Agendamento não encontrado.")
            : Resultado<AgendamentoDto>.Ok(item);
    }

    public async Task<Resultado<AgendamentoDto>> CriarAsync(AgendamentoCadastroDto dto, CancellationToken cancelamento = default)
    {
        await SimularEspera();
        var novo = new AgendamentoDto
        {
            Id = Guid.NewGuid(),
            ClienteId = dto.ClienteId,
            FuncionarioId = dto.FuncionarioId,
            ServicoId = dto.ServicoId,
            DataHoraInicio = dto.DataHoraInicio,
            DataHoraFim = dto.DataHoraInicio.AddMinutes(60),
            Status = "AGENDADO"
        };
        dados.Agendamentos.Add(novo);
        return Resultado<AgendamentoDto>.Ok(novo, "Agendamento criado.");
    }

    public async Task<Resultado<AgendamentoDto>> AtualizarAsync(AgendamentoEdicaoDto dto, CancellationToken cancelamento = default)
    {
        await SimularEspera();
        var indice = dados.Agendamentos.FindIndex(x => x.Id == dto.Id);
        if (indice < 0)
            return Resultado<AgendamentoDto>.Falha("Agendamento não encontrado.");

        var atual = dados.Agendamentos[indice];
        atual.ClienteId = dto.ClienteId;
        atual.FuncionarioId = dto.FuncionarioId;
        atual.ServicoId = dto.ServicoId;
        atual.DataHoraInicio = dto.DataHoraInicio;
        atual.DataHoraFim = dto.DataHoraInicio.AddMinutes(60);
        return Resultado<AgendamentoDto>.Ok(atual, "Agendamento atualizado.");
    }

    public async Task<Resultado> ExcluirAsync(Guid id, CancellationToken cancelamento = default)
    {
        await SimularEspera();
        var removido = dados.Agendamentos.RemoveAll(x => x.Id == id);
        return removido > 0 ? Resultado.Ok("Agendamento excluído.") : Resultado.Falha("Agendamento não encontrado.");
    }

    public async Task<Resultado> CancelarAsync(Guid id, CancellationToken cancelamento = default)
    {
        await SimularEspera();
        var item = dados.Agendamentos.FirstOrDefault(x => x.Id == id);
        if (item is null)
            return Resultado.Falha("Agendamento não encontrado.");
        item.Status = "CANCELADO";
        return Resultado.Ok("Agendamento cancelado.");
    }
}
