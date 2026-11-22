using SalaoAdmin.Comum;
using SalaoAdmin.Contratos;
using SalaoAdmin.DadosMock;
using SalaoAdmin.Dtos.Vendas;
using SalaoAdmin.Utilitarios;

namespace SalaoAdmin.Servicos.Mock;

public class VendaServicoMock(ArmazenamentoLocal dados) : IVendaServico
{
    public Task<Resultado<ListaPaginada<VendaDto>>> ListarAsync(FiltroVenda filtro, CancellationToken cancelamento = default)
    {
        VendaNumeroHelper.AplicarNumerosMock(dados.Vendas);

        var query = dados.Vendas.AsEnumerable();
        var buscaPorNumero = VendaNumeroHelper.TryParseNumeroBusca(filtro.Busca, out var numeroVenda);

        if (filtro.FuncionarioId.HasValue)
            query = query.Where(v => v.FuncionarioId == filtro.FuncionarioId.Value);
        if (filtro.ClienteId.HasValue)
            query = query.Where(v => v.ClienteId == filtro.ClienteId.Value);
        if (!buscaPorNumero && (filtro.Inicio.HasValue || filtro.Fim.HasValue))
            query = query.Where(v => VendaCalculoHelper.VendaNoPeriodo(v, filtro.Inicio, filtro.Fim));
        if (!string.IsNullOrWhiteSpace(filtro.TipoItem))
            query = query.Where(v => VendaCalculoHelper.VendaContemTipo(v, filtro.TipoItem));
        if (buscaPorNumero)
        {
            var ids = VendaNumeroHelper.BuscarIdsPorNumero(dados.Vendas, numeroVenda);
            query = query.Where(v => ids.Contains(v.Id));
        }

        var ordenado = query.OrderByDescending(v => v.DataHora).ToList();
        var total = ordenado.Count;
        var pagina = Math.Max(1, filtro.Pagina);
        var tamanho = Math.Max(1, filtro.ItensPorPagina);
        var itens = ordenado.Skip((pagina - 1) * tamanho).Take(tamanho).ToList();
        if (buscaPorNumero)
        {
            foreach (var item in itens)
                item.Numero = numeroVenda;
        }

        return Task.FromResult(Resultado<ListaPaginada<VendaDto>>.Ok(new ListaPaginada<VendaDto>
        {
            Itens = itens,
            Total = total,
            Pagina = pagina,
            ItensPorPagina = tamanho
        }));
    }

    public Task<Resultado<VendaDto>> ObterPorIdAsync(Guid id, CancellationToken cancelamento = default)
    {
        var venda = dados.Vendas.FirstOrDefault(v => v.Id == id);
        return Task.FromResult(venda is null
            ? Resultado<VendaDto>.Falha("Venda não encontrada.")
            : Resultado<VendaDto>.Ok(venda));
    }

    public Task<Resultado<VendaDto>> RegistrarAsync(VendaCadastroDto dto, CancellationToken cancelamento = default)
    {
        if (dto.Itens.Count == 0)
            return Task.FromResult(Resultado<VendaDto>.Falha("Informe ao menos um item na venda."));

        var itens = dto.Itens.Select(i => new ItemVendaDto
        {
            Id = Guid.NewGuid(),
            Tipo = i.Tipo,
            ProdutoId = i.ProdutoId,
            ServicoId = i.ServicoId,
            Quantidade = i.Quantidade,
            ValorUnitario = i.ValorUnitario
        }).ToList();

        var venda = new VendaDto
        {
            Id = Guid.NewGuid(),
            DataHora = dto.DataHora?.ToLocalTime() ?? DateTime.Now,
            FuncionarioId = dto.FuncionarioId,
            ClienteId = dto.ClienteId,
            Itens = itens,
            Total = itens.Sum(x => x.ValorUnitario * x.Quantidade)
        };

        dados.Vendas.Add(venda);
        VendaNumeroHelper.AplicarNumerosMock(dados.Vendas);
        return Task.FromResult(Resultado<VendaDto>.Ok(venda, "Venda registrada."));
    }
}
