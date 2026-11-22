using MudBlazor;

namespace SalaoAdmin.Servicos.Compartilhados;

public interface IServicoNotificacao
{
    void Sucesso(string mensagem);
    void Erro(string mensagem);
    void Aviso(string mensagem);
    void Informar(string mensagem);
}

public class ServicoNotificacao(ISnackbar snackbar) : IServicoNotificacao
{
    public void Sucesso(string mensagem) =>
        snackbar.Add(mensagem, Severity.Success, c => c.VisibleStateDuration = 4000);

    public void Erro(string mensagem) =>
        snackbar.Add(mensagem, Severity.Error, c => c.VisibleStateDuration = 6000);

    public void Aviso(string mensagem) =>
        snackbar.Add(mensagem, Severity.Warning, c => c.VisibleStateDuration = 5000);

    public void Informar(string mensagem) =>
        snackbar.Add(mensagem, Severity.Info, c => c.VisibleStateDuration = 4000);
}

