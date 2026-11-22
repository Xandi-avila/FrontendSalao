using FluentValidation;
using SalaoAdmin.Dtos.Servicos;

namespace SalaoAdmin.Validadores;

public class ValidadorServicoCadastro : AbstractValidator<ServicoCadastroDto>
{
    public ValidadorServicoCadastro()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome do serviço é obrigatório.")
            .MaximumLength(150);

        RuleFor(x => x.DuracaoMinutos)
            .GreaterThan(0).WithMessage("Informe a duração em minutos.");

        RuleFor(x => x.PrecoMinimo)
            .GreaterThan(0).WithMessage("Preço mínimo deve ser maior que zero.");

        RuleFor(x => x.CategoriaId)
            .NotEmpty().WithMessage("Categoria é obrigatória.");

        RuleFor(x => x.PercentualComissao)
            .InclusiveBetween(0m, 100m).WithMessage("Percentual de comissão deve ser entre 0 e 100%.");
    }
}

public class ValidadorServicoEdicao : AbstractValidator<ServicoEdicaoDto>
{
    public ValidadorServicoEdicao()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x).SetValidator(new ValidadorServicoCadastro());
    }
}

