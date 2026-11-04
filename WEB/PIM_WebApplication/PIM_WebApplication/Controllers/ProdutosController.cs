using Microsoft.AspNetCore.Mvc;
using PIM_WebApplication.Models;
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
        public async Task<IActionResult> Guitarras([FromQuery] string? ordem = null)
        {
            IEnumerable<Produto> produtos = await _produtoService.ObterPorCategoriaAsync("Guitarras");
            produtos = AplicarOrdenacao(produtos, ordem);
            ViewData["OrdemAtual"] = ordem ?? "populares";
            return View(produtos);
        }

        [HttpGet]
        public async Task<IActionResult> Violoes([FromQuery] string? ordem = null)
        {
            IEnumerable<Produto> produtos = await _produtoService.ObterPorCategoriaAsync("Violões");
            produtos = AplicarOrdenacao(produtos, ordem);
            ViewData["OrdemAtual"] = ordem ?? "populares";
            return View(produtos);
        }

        [HttpGet]
        public async Task<IActionResult> Teclados([FromQuery] string? ordem = null)
        {
            IEnumerable<Produto> produtos = await _produtoService.ObterPorCategoriaAsync("Teclados");
            produtos = AplicarOrdenacao(produtos, ordem);
            ViewData["OrdemAtual"] = ordem ?? "populares";
            return View(produtos);
        }

        [HttpGet]
        public async Task<IActionResult> Baterias([FromQuery] string? ordem = null)
        {
            IEnumerable<Produto> produtos = await _produtoService.ObterPorCategoriaAsync("Baterias");
            produtos = AplicarOrdenacao(produtos, ordem);
            ViewData["OrdemAtual"] = ordem ?? "populares";
            return View(produtos);
        }

        [HttpGet]
        public async Task<IActionResult> Outros([FromQuery] string? ordem = null)
        {
            IEnumerable<Produto> produtos = await _produtoService.ObterPorCategoriaAsync("Outros");
            produtos = AplicarOrdenacao(produtos, ordem);
            ViewData["OrdemAtual"] = ordem ?? "populares";
            return View(produtos);
        }

        [HttpGet]
        public async Task<IActionResult> Acessorios([FromQuery] string? ordem = null)
        {
            IEnumerable<Produto> produtos = await _produtoService.ObterPorCategoriaAsync("Acessórios");
            produtos = AplicarOrdenacao(produtos, ordem);
            ViewData["OrdemAtual"] = ordem ?? "populares";
            return View(produtos);
        }

        [HttpGet]
        public async Task<IActionResult> Detalhes(int id, [FromQuery] bool json = false)
        {
            var produto = await _produtoService.ObterPorIdAsync(id);
            if (produto == null || !produto.Ativo)
            {
                if (json)
                {
                    return NotFound(new { mensagem = "Produto nao encontrado ou indisponivel." });
                }
                TempData["MensagemToast"] = "Produto nao encontrado ou indisponivel no catalogo.";
                return RedirectToAction("Index", "Home");
            }

            if (json || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Ok(new
                {
                    produto.IdProduto,
                    produto.Modelo,
                    produto.TipoInstrumento,
                    produto.Preco,
                    Descricao = produto.Descricao ?? "Instrumento com sonoridade marcante, construcao de alto padrao e timbres selecionados para palco e estudio.",
                    produto.ImagemUrl,
                    EstoqueDisponivel = produto.Estoque?.QuantidadeDisponivel ?? 0
                });
            }

            return View(produto);
        }

        private static IEnumerable<Produto> AplicarOrdenacao(IEnumerable<Produto> produtos, string? ordem)
        {
            return ordem switch
            {
                "menor-preco" => produtos.OrderBy(p => p.Preco),
                "maior-preco" => produtos.OrderByDescending(p => p.Preco),
                _ => produtos
            };
        }
    }
}
