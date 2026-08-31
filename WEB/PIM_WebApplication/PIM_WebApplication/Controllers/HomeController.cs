using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PIM_WebApplication.Models;
using PIM_WebApplication.Services;
using PIM_WebApplication.ViewModels;

namespace PIM_WebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProdutoService _produtoService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IProdutoService produtoService, ILogger<HomeController> logger)
        {
            _produtoService = produtoService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var destaques = await _produtoService.ObterDestaquesAsync(6);

            var categorias = new List<CategoriaItemViewModel>
            {
                new() { Nome = "Camisetas", Icone = "👕" },
                new() { Nome = "Moletons", Icone = "🧥" },
                new() { Nome = "Programacao", Icone = "🖥️" },
                new() { Nome = "Linux", Icone = "🐧" },
                new() { Nome = "IA & Tech", Icone = "🤖" },
                new() { Nome = "Games", Icone = "🎮" }
            };

            var viewModel = new HomeViewModel
            {
                Destaques = destaques,
                Categorias = categorias
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Ofertas()
        {
            var produtos = await _produtoService.ObterOfertasAsync();
            var viewModel = new OfertasViewModel
            {
                Produtos = produtos,
                PercentualDesconto = 50m
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Sobre()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Privacidade()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Acessibilidade()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
