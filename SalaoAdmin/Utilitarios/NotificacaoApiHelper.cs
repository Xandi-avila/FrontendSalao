using SalaoAdmin.Comum;
using SalaoAdmin.Servicos.Compartilhados;

namespace SalaoAdmin.Utilitarios;

public static class NotificacaoApiHelper
{
    public static void NotificarFalha(
        this IServicoNotificacao notificacoes,
        Resultado resultado,
        string mensagemPadrao = "Não foi possível concluir a operação.")
    {
        var erros = resultado.Erros
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Distinct()
            .ToList();

        if (erros.Count > 0)
        {
            foreach (var erro in erros)
                notificacoes.Erro(erro);
            return;
        }

        notificacoes.Erro(resultado.Mensagem ?? mensagemPadrao);
    }
}
