using FluentValidation;
using SalaoAdmin.Dtos.Funcionarios;

namespace SalaoAdmin.Validadores;

public class ValidadorFuncionarioCadastro : AbstractValidator<FuncionarioCadastroDto>
{
    public ValidadorFuncionarioCadastro()
    {
        RuleFor(x => x.NomeCompleto).NotEmpty().WithMessage("Informe o nome completo.").MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("E-mail inválido.");
        RuleFor(x => x.Telefone).NotEmpty().MinimumLength(14).WithMessage("Telefone incompleto.");
        RuleFor(x => x.ProfissaoCargo).NotEmpty().WithMessage("Informe o cargo.");
        RuleFor(x => x.Endereco).NotEmpty().WithMessage("Informe o endereço.");
    }
}

public class ValidadorFuncionarioEdicao : AbstractValidator<FuncionarioEdicaoDto>
{
    public ValidadorFuncionarioEdicao()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x).SetValidator(new ValidadorFuncionarioCadastro());
    }
}

