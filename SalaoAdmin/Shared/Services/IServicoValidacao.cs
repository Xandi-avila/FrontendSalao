using FluentValidation;
using FluentValidation.Results;

namespace SalaoAdmin.Servicos.Compartilhados;

public interface IServicoValidacao
{
    Task<ValidationResult> ValidarAsync<T>(T modelo);
    Dictionary<string, string> ParaErros(ValidationResult resultado);
}

public class ServicoValidacao(IServiceProvider provedor) : IServicoValidacao
{
    public async Task<ValidationResult> ValidarAsync<T>(T modelo)
    {
        var validador = provedor.GetService<IValidator<T>>();
        if (validador is null)
            return new ValidationResult();

        return await validador.ValidateAsync(modelo);
    }

    public Dictionary<string, string> ParaErros(ValidationResult resultado) =>
        resultado.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.First().ErrorMessage);
}

