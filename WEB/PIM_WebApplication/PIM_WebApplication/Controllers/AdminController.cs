using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PIM_WebApplication.Models;
using PIM_WebApplication.Services;
using PIM_WebApplication.ViewModels;

namespace PIM_WebApplication.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IProdutoService _produtoService;
        private readonly IEstoqueService _estoqueService;

        public AdminController(IProdutoService produtoService, IEstoqueService estoqueService)
        {
            _produtoService = produtoService;
            _estoqueService = estoqueService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var produtos = await _produtoService.ObterTodosAsync();
            var viewModel = new AdminViewModel
            {
                ProdutosCadastrados = produtos,
                NovoProduto = new NovoProdutoViewModel(),
                AdicionarEstoque = new AdicionarEstoqueViewModel()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CadastrarProduto(NovoProdutoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var produtos = await _produtoService.ObterTodosAsync();
                var vm = new AdminViewModel
                {
                    ProdutosCadastrados = produtos,
                    NovoProduto = model,
                    AdicionarEstoque = new AdicionarEstoqueViewModel()
                };
                return View("Index", vm);
            }

            var produto = new Produto
            {
                TipoInstrumento = model.Nome.Trim(),
                Preco = model.Preco,
                Modelo = !string.IsNullOrWhiteSpace(model.Modelo) ? model.Modelo : (model.Tamanho ?? string.Empty),
                ImagemUrl = !string.IsNullOrWhiteSpace(model.ImagemUrl) ? model.ImagemUrl.Trim() : null
            };

            await _produtoService.CriarProdutoAsync(produto, model.EstoqueInicial);
            TempData["MensagemToast"] = $"Instrumento '{produto.TipoInstrumento}' cadastrado com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdicionarEstoque(AdicionarEstoqueViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var produtos = await _produtoService.ObterTodosAsync();
                var vm = new AdminViewModel
                {
                    ProdutosCadastrados = produtos,
                    NovoProduto = new NovoProdutoViewModel(),
                    AdicionarEstoque = model
                };
                return View("Index", vm);
            }

            try
            {
                int novaQuantidade = await _estoqueService.AdicionarEstoqueAsync(model.IdProduto, model.Quantidade);
                TempData["MensagemToast"] = $"Estoque atualizado com sucesso! Novo saldo: {novaQuantidade} un.";
            }
            catch (Exception ex)
            {
                TempData["MensagemToast"] = $"Erro ao atualizar estoque: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirProduto(int id)
        {
            try
            {
                bool excluido = await _produtoService.ExcluirProdutoAsync(id);
                if (excluido)
                {
                    TempData["MensagemToast"] = "Produto removido com sucesso.";
                }
                else
                {
                    TempData["MensagemToast"] = "Produto não encontrado.";
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemToast"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
