using FluentValidation;
using SalaoAdmin.Dtos.Produtos;

namespace SalaoAdmin.Validadores;

public class ValidadorProdutoCadastro : AbstractValidator<ProdutoCadastroDto>
{
    public ValidadorProdutoCadastro()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome do produto é obrigatório.")
            .MaximumLength(150);

        RuleFor(x => x.Valor)
            .GreaterThan(0).WithMessage("Valor deve ser maior que zero.");
    }
}

public class ValidadorProdutoEdicao : AbstractValidator<ProdutoEdicaoDto>
{
    public ValidadorProdutoEdicao()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x).SetValidator(new ValidadorProdutoCadastro());
    }
}

