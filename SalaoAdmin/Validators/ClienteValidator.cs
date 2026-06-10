using FluentValidation;
using SalaoAdmin.Dtos.Clientes;
using SalaoAdmin.Utilitarios;

namespace SalaoAdmin.Validadores;

public class ValidadorClienteCadastro : AbstractValidator<ClienteCadastroDto>
{
    public ValidadorClienteCadastro()
    {
        RuleFor(x => x.NomeCompleto)
            .NotEmpty().WithMessage("Nome completo é obrigatório.")
            .MaximumLength(200);

        RuleFor(x => x.WhatsApp)
            .NotEmpty().WithMessage("WhatsApp é obrigatório.")
            .MinimumLength(14).WithMessage("WhatsApp inválido.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email)).WithMessage("E-mail inválido.");

        RuleFor(x => x.Endereco)
            .NotEmpty().WithMessage("Endereço é obrigatório.");

        RuleFor(x => x.DataNascimento)
            .Must(data => ValidacaoDatas.MensagemErroNascimento(data) is null)
            .WithMessage(x => ValidacaoDatas.MensagemErroNascimento(x.DataNascimento)!);
    }
}

public class ValidadorClienteEdicao : AbstractValidator<ClienteEdicaoDto>
{
    public ValidadorClienteEdicao()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.NomeCompleto)
            .NotEmpty().WithMessage("Nome completo é obrigatório.")
            .MaximumLength(200);

        RuleFor(x => x.WhatsApp)
            .NotEmpty().WithMessage("WhatsApp é obrigatório.")
            .MinimumLength(14).WithMessage("WhatsApp inválido.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email)).WithMessage("E-mail inválido.");

        RuleFor(x => x.Endereco)
            .NotEmpty().WithMessage("Endereço é obrigatório.");

        RuleFor(x => x.DataNascimento)
            .Must(data => ValidacaoDatas.MensagemErroNascimento(data) is null)
            .WithMessage(x => ValidacaoDatas.MensagemErroNascimento(x.DataNascimento)!);
    }
}
