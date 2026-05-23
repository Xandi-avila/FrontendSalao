using SalaoAdmin.Dtos.Produtos;

namespace SalaoAdmin.Contratos;

public interface IProdutoServico : IServicoCrud<ProdutoDto, ProdutoCadastroDto, ProdutoEdicaoDto>;

