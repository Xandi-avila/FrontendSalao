using SalaoAdmin.Comum;
using SalaoAdmin.Dtos.Vendas;

namespace SalaoAdmin.Contratos;

public interface IVendaServico
{
    Task<Resultado<ListaPaginada<VendaDto>>> ListarAsync(FiltroVenda filtro, CancellationToken cancelamento = default);
    Task<Resultado<VendaDto>> ObterPorIdAsync(Guid id, CancellationToken cancelamento = default);
    Task<Resultado<VendaDto>> RegistrarAsync(VendaCadastroDto dto, CancellationToken cancelamento = default);
}
