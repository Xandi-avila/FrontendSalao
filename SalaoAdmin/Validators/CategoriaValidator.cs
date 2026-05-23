using FluentValidation;
using SalaoAdmin.Dtos.Categorias;

namespace SalaoAdmin.Validadores;

public class ValidadorCategoriaCadastro : AbstractValidator<CategoriaCadastroDto>
{
    public ValidadorCategoriaCadastro()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome da categoria é obrigatório.")
            .MaximumLength(100);

        RuleFor(x => x.ServicoIds)
            .NotEmpty().WithMessage("Selecione ao menos um serviço vinculado.");
    }
}

public class ValidadorCategoriaEdicao : AbstractValidator<CategoriaEdicaoDto>
{
    public ValidadorCategoriaEdicao()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x).SetValidator(new ValidadorCategoriaCadastro());
    }
}

