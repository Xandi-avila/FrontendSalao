using SalaoAdmin.Comum;

namespace SalaoAdmin.Servicos.Api;

internal static class ConversoresResultado
{
    public static Resultado<T> ParaResultado<T>(this ApiResult<T> api)
    {
        return api.Sucesso
            ? Resultado<T>.Ok(api.Dados!, api.Mensagem)
            : Resultado<T>.Falha(api.Erros.Count > 0 ? api.Erros : [api.Mensagem ?? "Falha na operação."]);
    }

    public static Resultado ParaResultadoBase<T>(this ApiResult<T> api)
    {
        return api.Sucesso
            ? Resultado.Ok(api.Mensagem)
            : Resultado.Falha(api.Erros.Count > 0 ? api.Erros : [api.Mensagem ?? "Falha na operação."]);
    }
}
