namespace SalaoAdmin.Servicos.Compartilhados;

public interface ITratadorErros
{
    Task TratarAsync(Exception excecao, string? contexto = null);
}

public class TratadorErros(IServicoNotificacao notificacoes) : ITratadorErros
{
    public Task TratarAsync(Exception excecao, string? contexto = null)
    {
        var texto = string.IsNullOrWhiteSpace(contexto)
            ? excecao.Message
            : $"{contexto}: {excecao.Message}";

        notificacoes.Erro(texto);
        return Task.CompletedTask;
    }
}

