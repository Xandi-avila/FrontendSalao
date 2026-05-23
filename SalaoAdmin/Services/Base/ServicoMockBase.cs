using SalaoAdmin.Comum;

namespace SalaoAdmin.Servicos.Base;

public abstract class ServicoMockBase<TDto> where TDto : class
{
    protected static async Task<Resultado<ListaPaginada<TDto>>> PaginarAsync(
        IEnumerable<TDto> origem,
        FiltroPaginacao filtro,
        Func<TDto, bool>? filtrar = null,
        Func<TDto, object>? ordenar = null)
    {
        await Task.Delay(250);

        var consulta = origem.AsEnumerable();
        if (filtrar is not null)
            consulta = consulta.Where(filtrar);

        if (!string.IsNullOrWhiteSpace(filtro.OrdenarPor) && ordenar is not null)
        {
            consulta = filtro.OrdemDecrescente
                ? consulta.OrderByDescending(ordenar)
                : consulta.OrderBy(ordenar);
        }

        var lista = consulta.ToList();
        var itens = lista.Skip(filtro.Deslocamento).Take(filtro.ItensPorPagina).ToList();

        return Resultado<ListaPaginada<TDto>>.Ok(new ListaPaginada<TDto>
        {
            Itens = itens,
            Total = lista.Count,
            Pagina = filtro.Pagina,
            ItensPorPagina = filtro.ItensPorPagina
        });
    }

    protected static Task SimularEspera() => Task.Delay(150);
}

