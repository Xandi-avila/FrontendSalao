using FluentValidation;
using SalaoAdmin.Dtos.Funcionarios;
using SalaoAdmin.Utilitarios;

namespace SalaoAdmin.Validadores;

public class ValidadorFuncionarioCadastro : AbstractValidator<FuncionarioCadastroDto>
{
    public ValidadorFuncionarioCadastro()
    {
        RuleFor(x => x.NomeCompleto).NotEmpty().WithMessage("Informe o nome completo.").MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("E-mail inválido.");
        RuleFor(x => x.Senha).NotEmpty().MinimumLength(6).WithMessage("Senha deve ter no mínimo 6 caracteres.");
        RuleFor(x => x.Telefone).NotEmpty().MinimumLength(14).WithMessage("Telefone incompleto.");
        RuleFor(x => x.CPF)
            .Must(cpf => string.IsNullOrWhiteSpace(cpf) || FormatacaoCampos.SoNumeros(cpf).Length == 11)
            .WithMessage("CPF deve conter 11 dígitos.");
        RuleFor(x => x.Profissoes).NotEmpty().WithMessage("Informe ao menos uma profissão.");
        RuleFor(x => x.Endereco).NotEmpty().WithMessage("Informe o endereço.");
        RuleFor(x => x.PercentualComissaoProduto)
            .InclusiveBetween(0m, 100m).WithMessage("Comissão sobre produtos deve ser entre 0 e 100%.");
        RuleFor(x => x.PercentualComissaoServico)
            .InclusiveBetween(0m, 100m).WithMessage("Comissão sobre serviços deve ser entre 0 e 100%.");

        RuleFor(x => x.DataAdmissao)
            .Must(data => ValidacaoDatas.MensagemErroAdmissao(data) is null)
            .WithMessage(x => ValidacaoDatas.MensagemErroAdmissao(x.DataAdmissao)!);

        RuleFor(x => x.DataNascimento)
            .Must(data => ValidacaoDatas.MensagemErroNascimento(data) is null)
            .WithMessage(x => ValidacaoDatas.MensagemErroNascimento(x.DataNascimento)!);
    }
}

public class ValidadorFuncionarioEdicao : AbstractValidator<FuncionarioEdicaoDto>
{
    public ValidadorFuncionarioEdicao()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NomeCompleto).NotEmpty().WithMessage("Informe o nome completo.").MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("E-mail inválido.");
        RuleFor(x => x.Telefone).NotEmpty().MinimumLength(14).WithMessage("Telefone incompleto.");
        RuleFor(x => x.CPF)
            .Must(cpf => string.IsNullOrWhiteSpace(cpf) || FormatacaoCampos.SoNumeros(cpf).Length == 11)
            .WithMessage("CPF deve conter 11 dígitos.");
        RuleFor(x => x.Profissoes).NotEmpty().WithMessage("Informe ao menos uma profissão.");
        RuleFor(x => x.Endereco).NotEmpty().WithMessage("Informe o endereço.");
        RuleFor(x => x.PercentualComissaoProduto)
            .InclusiveBetween(0m, 100m).WithMessage("Comissão sobre produtos deve ser entre 0 e 100%.");
        RuleFor(x => x.PercentualComissaoServico)
            .InclusiveBetween(0m, 100m).WithMessage("Comissão sobre serviços deve ser entre 0 e 100%.");
        RuleFor(x => x.Senha)
            .MinimumLength(6)
            .When(x => !string.IsNullOrWhiteSpace(x.Senha))
            .WithMessage("Nova senha deve ter no mínimo 6 caracteres.");

        RuleFor(x => x.DataAdmissao)
            .Must(data => ValidacaoDatas.MensagemErroAdmissao(data) is null)
            .WithMessage(x => ValidacaoDatas.MensagemErroAdmissao(x.DataAdmissao)!);

        RuleFor(x => x.DataNascimento)
            .Must(data => ValidacaoDatas.MensagemErroNascimento(data) is null)
            .WithMessage(x => ValidacaoDatas.MensagemErroNascimento(x.DataNascimento)!);
    }
}
