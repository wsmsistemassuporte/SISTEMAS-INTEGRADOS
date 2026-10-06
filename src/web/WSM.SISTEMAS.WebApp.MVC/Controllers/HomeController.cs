using Microsoft.AspNetCore.Mvc;
using WSM.SISTEMAS.WebApp.MVC.Models.Produtos;

namespace WSM.SISTEMAS.WebApp.MVC.Controllers
{
    public class HomeController : MainController
    {
        public IActionResult Index()
        {
            ViewBag.Destaques = new List<ProductItemViewModel>();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statusCode = StatusCodes.Status500InternalServerError)
        {
            return View(CriarErrorViewModel(statusCode));
        }
    }
}
