using SalaoAdmin.Comum;
using SalaoAdmin.Contratos;
using SalaoAdmin.DadosMock;
using SalaoAdmin.Dtos.Agenda;

namespace SalaoAdmin.Servicos.Mock;

public class AgendaServicoMock(ArmazenamentoLocal dados) : IAgendaServico
{
    public Task<Resultado<List<JanelaAgendaDto>>> ListarPorFuncionarioAsync(Guid funcionarioId, CancellationToken cancelamento = default)
    {
        var lista = dados.JanelasAgenda
            .Where(x => x.FuncionarioId == funcionarioId)
            .ToList();
        return Task.FromResult(Resultado<List<JanelaAgendaDto>>.Ok(lista));
    }

    public Task<Resultado<List<JanelaAgendaDto>>> DefinirAgendaAsync(Guid funcionarioId, DefinirAgendaDto dto, CancellationToken cancelamento = default)
    {
        dados.JanelasAgenda.RemoveAll(x => x.FuncionarioId == funcionarioId);
        foreach (var janela in dto.Janelas)
        {
            janela.Id = janela.Id == Guid.Empty ? Guid.NewGuid() : janela.Id;
            janela.FuncionarioId = funcionarioId;
            dados.JanelasAgenda.Add(janela);
        }

        var lista = dados.JanelasAgenda
            .Where(x => x.FuncionarioId == funcionarioId)
            .ToList();

        return Task.FromResult(Resultado<List<JanelaAgendaDto>>.Ok(lista, "Agenda atualizada."));
    }

    public Task<Resultado> ExcluirJanelaAsync(Guid janelaId, CancellationToken cancelamento = default)
    {
        var removido = dados.JanelasAgenda.RemoveAll(x => x.Id == janelaId);
        return Task.FromResult(removido > 0
            ? Resultado.Ok("Janela removida.")
            : Resultado.Falha("Janela não encontrada."));
    }
}
