using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WSM.SISTEMAS.WebApp.MVC.Models;

namespace WSM.SISTEMAS.WebApp.MVC.Controllers
{
    public abstract class MainController : Controller
    {
        protected void AdicionarErroProcessamento(string erro)
        {
            ModelState.AddModelError(string.Empty, erro);
        }

        protected void AdicionarErros(ServiceResult result)
        {
            var errors = result.Errors.Count > 0
                ? result.Errors
                : [ObterMensagemUsuario(result)];

            foreach (var error in errors)
            {
                AdicionarErroProcessamento(error);
            }
        }

        private static string ObterMensagemUsuario(ServiceResult result)
        {
            return string.IsNullOrWhiteSpace(result.MensagemUsuario)
                ? "Nao foi possivel processar sua solicitacao."
                : result.MensagemUsuario;
        }

        protected IActionResult RedirectToLocal(string? returnUrl, string fallbackAction = "Index", string fallbackController = "Home")
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(fallbackAction, fallbackController);
        }

        protected ErrorViewModel CriarErrorViewModel(int statusCode)
        {
            return new ErrorViewModel
            {
                ErrorCode = statusCode,
                Titulo = ObterTituloErro(statusCode),
                Mensagem = ObterMensagemErro(statusCode),
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };
        }

        private static string ObterTituloErro(int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status404NotFound => "Pagina nao encontrada",
                StatusCodes.Status403Forbidden => "Acesso negado",
                StatusCodes.Status401Unauthorized => "Autenticacao necessaria",
                _ => "Ops! Ocorreu um erro inesperado"
            };
        }

        private static string ObterMensagemErro(int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status404NotFound => "A pagina que voce tentou acessar nao existe ou foi movida.",
                StatusCodes.Status403Forbidden => "Voce nao tem permissao para acessar este recurso.",
                StatusCodes.Status401Unauthorized => "Faca login para continuar.",
                _ => "Nao foi possivel concluir a sua solicitacao. Por favor, tente novamente."
            };
        }
    }
}
