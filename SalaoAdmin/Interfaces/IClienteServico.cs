using SalaoAdmin.Dtos.Clientes;

namespace SalaoAdmin.Contratos;

public interface IClienteServico : IServicoCrud<ClienteDto, ClienteCadastroDto, ClienteEdicaoDto>;

