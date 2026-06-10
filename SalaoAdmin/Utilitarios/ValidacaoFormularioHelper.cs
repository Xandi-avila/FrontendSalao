using SalaoAdmin.Servicos.Compartilhados;

namespace SalaoAdmin.Utilitarios;

public static class ValidacaoFormularioHelper
{
    public static async Task<Dictionary<string, string>> ValidarAntesDeSalvarAsync<T>(
        IServicoValidacao validacao,
        T modelo,
        Func<Dictionary<string, string>>? coletarErrosDatas = null,
        Func<Task>? sincronizarDatas = null)
    {
        if (sincronizarDatas is not null)
            await sincronizarDatas();

        var erros = coletarErrosDatas?.Invoke() ?? new Dictionary<string, string>();
        var resultado = await validacao.ValidarAsync(modelo);

        foreach (var (campo, mensagem) in validacao.ParaErros(resultado))
            erros[campo] = mensagem;

        return erros;
    }
}
