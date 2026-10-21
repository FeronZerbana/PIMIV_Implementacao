using Microsoft.AspNetCore.Mvc;

namespace PIM_WebApplication.Controllers
{
    public class ProdutosController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Guitarras()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Violoes()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Teclados()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Baterias()
        {
            return View();
        }
    }
}
