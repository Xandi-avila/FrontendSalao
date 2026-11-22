namespace SalaoAdmin.Comum;

public class BaseResponse
{
    public bool Sucesso { get; init; }
    public string? Mensagem { get; init; }
    public IReadOnlyList<string> Erros { get; init; } = [];
}

public class ApiResult<T> : BaseResponse
{
    public T? Dados { get; init; }

    public static ApiResult<T> Ok(T? dados, string? mensagem = null) => new()
    {
        Sucesso = true,
        Dados = dados,
        Mensagem = mensagem
    };

    public static ApiResult<T> Falha(string mensagem, IReadOnlyList<string>? erros = null) => new()
    {
        Sucesso = false,
        Mensagem = mensagem,
        Erros = erros ?? [mensagem]
    };
}
