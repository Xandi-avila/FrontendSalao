namespace SalaoAdmin.Comum;

public class Resultado
{
    public bool Sucesso { get; init; }
    public string? Mensagem { get; init; }
    public IReadOnlyList<string> Erros { get; init; } = [];

    public static Resultado Ok(string? mensagem = null) => new()
    {
        Sucesso = true,
        Mensagem = mensagem
    };

    public static Resultado Falha(string mensagem) => new()
    {
        Sucesso = false,
        Mensagem = mensagem,
        Erros = [mensagem]
    };

    public static Resultado Falha(IEnumerable<string> erros)
    {
        var lista = erros.ToList();
        return new Resultado
        {
            Sucesso = false,
            Mensagem = lista.FirstOrDefault(),
            Erros = lista
        };
    }
}

public class Resultado<T> : Resultado
{
    public T? Dados { get; init; }

    public static Resultado<T> Ok(T dados, string? mensagem = null) => new()
    {
        Sucesso = true,
        Dados = dados,
        Mensagem = mensagem
    };

    public new static Resultado<T> Falha(string mensagem) => new()
    {
        Sucesso = false,
        Mensagem = mensagem,
        Erros = [mensagem]
    };

    public new static Resultado<T> Falha(IEnumerable<string> erros)
    {
        var lista = erros.ToList();
        return new Resultado<T>
        {
            Sucesso = false,
            Mensagem = lista.FirstOrDefault(),
            Erros = lista
        };
    }
}

