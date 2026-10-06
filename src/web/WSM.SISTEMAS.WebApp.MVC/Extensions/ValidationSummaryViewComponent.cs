using Microsoft.AspNetCore.Mvc;

namespace WSM.SISTEMAS.WebApp.MVC.Extensions
{
    /// <summary>
    /// View Component que substitui o asp-validation-summary padrão.
    /// Exibe os erros do ModelState com o estilo visual da Forja (Tailwind).
    ///
    /// Uso:
    ///   @await Component.InvokeAsync("ValidationSummary")           // ModelOnly (padrão)
    ///   @await Component.InvokeAsync("ValidationSummary", new { modelOnly = false })  // All
    /// </summary>
    [ViewComponent(Name = "ValidationSummary")]
    public class ValidationSummaryViewComponent : ViewComponent
    {
        /// <summary>
        /// Renderiza os erros de validação do ModelState.
        /// </summary>
        /// <param name="modelOnly">
        /// true (padrão): apenas erros de nível do modelo (key vazia).
        /// false: todos os erros do ModelState.
        /// </param>
        public IViewComponentResult Invoke(bool modelOnly = true)
        {
            var modelState = ViewContext.ModelState;

            if (modelState.IsValid)
            {
                return Content(string.Empty);
            }

            var errors = modelOnly
                ? modelState
                    .Where(ms => string.IsNullOrEmpty(ms.Key))
                    .SelectMany(ms => ms.Value!.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(msg => !string.IsNullOrWhiteSpace(msg))
                : modelState
                    .SelectMany(ms => ms.Value!.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(msg => !string.IsNullOrWhiteSpace(msg));

            return View(errors.Distinct().ToList());
        }
    }
}
