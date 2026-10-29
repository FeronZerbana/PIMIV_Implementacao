using Microsoft.AspNetCore.Mvc;
using PIM_WebApplication.Services;

namespace PIM_WebApplication.Controllers
{
    public class ProdutosController : Controller
    {
        private readonly IProdutoService _produtoService;

        public ProdutosController(IProdutoService produtoService)
        {
            _produtoService = produtoService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Guitarras()
        {
            var produtos = await _produtoService.ObterPorCategoriaAsync("Guitarras");
            return View(produtos);
        }

        [HttpGet]
        public async Task<IActionResult> Violoes()
        {
            var produtos = await _produtoService.ObterPorCategoriaAsync("Violões");
            return View(produtos);
        }

        [HttpGet]
        public async Task<IActionResult> Teclados()
        {
            var produtos = await _produtoService.ObterPorCategoriaAsync("Teclados");
            return View(produtos);
        }

        [HttpGet]
        public async Task<IActionResult> Baterias()
        {
            var produtos = await _produtoService.ObterPorCategoriaAsync("Baterias");
            return View(produtos);
        }

        [HttpGet]
        public async Task<IActionResult> Outros()
        {
            var produtos = await _produtoService.ObterPorCategoriaAsync("Outros");
            return View(produtos);
        }

        [HttpGet]
        public async Task<IActionResult> Acessorios()
        {
            var produtos = await _produtoService.ObterPorCategoriaAsync("Acessórios");
            return View(produtos);
        }
    }
}
